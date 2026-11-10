using AutomotiveDMS.Application.Auditing;
using AutomotiveDMS.Application.Interfaces.Services;
using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Entities;
using AutomotiveDMS.Domain.Enums;
using AutomotiveDMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Interceptors
{
    public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private static readonly HashSet<Type> NotAudited =
        [
            typeof(AuditLog),
            typeof(CommunicationLog),
            typeof(VehicleStatusHistory),
            typeof(VehicleZoneHistory),
            typeof(VehiclePriceHistory)
        ];

        private static readonly IReadOnlyDictionary<string, string> NoRoles = new Dictionary<string, string>();

        private readonly AuditChangeDetector _detector;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<AuditSaveChangesInterceptor> _logger;


        private readonly List<(EntityEntry Entry, string EntityName)> _pendingCreated = [];
        private string _actor = CurrentUserDefaults.System;
        private string? _ipAddress;
        private DateTime _timestamp;
        public bool _writingDeferred;

        public AuditSaveChangesInterceptor(
            AuditChangeDetector detector,
            ICurrentUserService currentUser,
            ILogger<AuditSaveChangesInterceptor> logger)
        {
            _detector = detector;
            _currentUser = currentUser;
            _logger = logger;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is ApplicationDbContext db && !_writingDeferred)
            {
                var roleNames = HasRolesChanges(db)
                    ? await db.Roles.AsNoTracking().ToDictionaryAsync(r => r.Id, r => r.Name ?? r.Id, cancellationToken)
                    : NoRoles;

                Prepare(db, roleNames);
            }

            return result;
        }

        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is ApplicationDbContext db && !_writingDeferred && _pendingCreated.Count > 0)
            {
                var logs = TakeDeferredLogs();
                try
                {
                    _writingDeferred = true;
                    db.AuditLogs.AddRange(logs);
                    await db.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    HandleDeferredFailure(db, logs, ex);
                }
                finally
                {
                    _writingDeferred = false;
                }
            }

            return result;
        }

        public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            _pendingCreated.Clear();
            return Task.CompletedTask;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (eventData.Context is ApplicationDbContext db && !_writingDeferred)
            {
                var roleNames = HasRolesChanges(db)
                    ? db.Roles.AsNoTracking().ToDictionary(r => r.Id, r => r.Name ?? r.Id)
                    : NoRoles;

                Prepare(db, roleNames);
            }

            return result;
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            if (eventData.Context is ApplicationDbContext db && !_writingDeferred && _pendingCreated.Count > 0)
            {
                var logs = TakeDeferredLogs();
                try
                {
                    _writingDeferred = true;
                    db.AuditLogs.AddRange(logs);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    HandleDeferredFailure(db, logs, ex);
                }
                finally
                {
                    _writingDeferred = false;
                }
            }

            return result;
        }

        public override void SaveChangesFailed(DbContextErrorEventData eventData) => _pendingCreated.Clear();

        private void Prepare(ApplicationDbContext db, IReadOnlyDictionary<string, string> roleNames)
        {
            _pendingCreated.Clear();
            _actor = _currentUser.UserId ?? CurrentUserDefaults.System;
            _ipAddress = _currentUser.IpAddress;
            _timestamp = DateTime.UtcNow;

            var changes = new List<AuditChange>();

            foreach (var entry in db.ChangeTracker.Entries().ToList())
            {
                if (entry.State is EntityState.Detached or EntityState.Unchanged)
                    continue;

                if (entry.Entity is AuditLog && entry.State is EntityState.Modified or EntityState.Deleted)
                    throw new InvalidOperationException("Audit log entries are immutable and cannot be modified or deleted.");

                StampAuditFields(entry);

                if (entry.Entity is IdentityUserRole<string> userRole)
                {
                    changes.AddRange(DescribeRoleChange(entry.State, userRole, roleNames));
                    continue;
                }

                if (!IsAudited(entry))
                    continue;

                if (entry.State == EntityState.Added && HasTemporaryKey(entry))
                {
                    _pendingCreated.Add((entry, EntityNameOf(entry)));
                    continue;
                }

                changes.AddRange(_detector.Detect(ToSnapshot(entry)));
            }

            db.AuditLogs.AddRange(changes.Select(ToAuditLog));
        }

        private void StampAuditFields(EntityEntry entry)
        {
            if (entry.Entity is not AuditableEntity)
                return;

            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(AuditableEntity.CreatedBy)).CurrentValue = _actor;
                entry.Property(nameof(AuditableEntity.CreatedDate)).CurrentValue = _timestamp;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(AuditableEntity.ModifiedBy)).CurrentValue = _actor;
                entry.Property(nameof(AuditableEntity.ModifiedDate)).CurrentValue = _timestamp;

                entry.Property(nameof(AuditableEntity.CreatedBy)).IsModified = false;
                entry.Property(nameof(AuditableEntity.CreatedDate)).IsModified = false;
            }
        }

        private static IEnumerable<AuditChange> DescribeRoleChange(
            EntityState state, IdentityUserRole<string> userRole, IReadOnlyDictionary<string, string> roleNames)
        {
            var role = roleNames.GetValueOrDefault(userRole.RoleId, userRole.RoleId);

            return state switch
            {
                EntityState.Added => [new AuditChange("User", userRole.UserId, AuditAction.Updated, "Role", null, role)],
                EntityState.Deleted => [new AuditChange("User", userRole.UserId, AuditAction.Updated, "Role", role, null)],
                _                   => []
            };
        }

        private List<AuditLog> TakeDeferredLogs()
        {
            var logs = _pendingCreated
                .Select(p => new AuditChange(p.EntityName, KeyOf(p.Entry), AuditAction.Created, null, null, null))
                .Select(ToAuditLog)
                .ToList();
            
            _pendingCreated.Clear();
            return logs;
        }

        private void HandleDeferredFailure(ApplicationDbContext db, List<AuditLog> logs, Exception ex)
        {
            _logger.LogError(ex, "Failed to write {Count} deferred 'Created' audit entries", logs.Count);
            foreach (var log in logs)
                db.Entry(log).State = EntityState.Detached;
        }

        private AuditLog ToAuditLog(AuditChange change) => new()
        {
            EntityName = change.EntityName,
            EntityId = change.EntityId,
            Action = change.Action,
            FieldName = change.FieldName,
            OldValue = change.OldValue,
            NewValue = change.NewValue,
            ChangedBy = _actor,
            ChangedDate = _timestamp,
            IpAddress = _ipAddress,
        };

        private static EntitySnapshot ToSnapshot(EntityEntry entry) => new(
            EntityNameOf(entry),
            KeyOf(entry),
            entry.State switch
            {
                EntityState.Added => EntityChangeState.Added,
                EntityState.Deleted => EntityChangeState.Deleted,
                _ => EntityChangeState.Modified
            },
            entry.Properties
                .Where(p => !p.Metadata.IsPrimaryKey() && p.Metadata.ValueGenerated != ValueGenerated.OnAddOrUpdate)
                .Select(p => new PropertySnapshot(
                    p.Metadata.Name,
                    entry.State == EntityState.Added ? null : p.OriginalValue,
                    p.CurrentValue))
                .ToList());

        private static bool IsAudited(EntityEntry entry) =>
            entry.Entity is ApplicationUser ||
            (entry.Entity is BaseEntity && !NotAudited.Contains(entry.Metadata.ClrType));

        private static bool HasRolesChanges(ApplicationDbContext db) =>
            db.ChangeTracker.Entries<IdentityUserRole<string>>()
               .Any(e => e.State is EntityState.Added or EntityState.Deleted);

        private static bool HasTemporaryKey(EntityEntry entry) =>
            entry.Properties.Any(p => p.Metadata.IsPrimaryKey() && p.IsTemporary);

        private static string EntityNameOf(EntityEntry entry) =>
            entry.Entity is ApplicationUser ? "User" : entry.Metadata.ClrType.Name;

        private static string KeyOf(EntityEntry entry) =>
            string.Join(",", entry.Properties
                .Where(p => p.Metadata.IsPrimaryKey())
                .Select(p => Convert.ToString(p.CurrentValue, CultureInfo.InvariantCulture)));
    }
}
