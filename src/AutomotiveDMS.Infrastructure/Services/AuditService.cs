using AutomotiveDMS.Application.Common.Models;
using AutomotiveDMS.Application.DTOs.Audit;
using AutomotiveDMS.Application.Interfaces.Services;
using AutomotiveDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Services
{
    public sealed class AuditService : IAuditService
    {
        private const int MaxPageSize = 100;
        private readonly ApplicationDbContext _db;

        public AuditService(ApplicationDbContext db) => _db = db;

        public async Task<IReadOnlyList<string>> GetEntityNamesAsync(CancellationToken cancellationToken = default) =>
            await _db.AuditLogs.AsNoTracking()
                .Select(a => a.EntityName)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync(cancellationToken);

        public async Task<PagedResult<AuditLogEntryDto>> SearchAsync(AuditLogQuery query, CancellationToken cancellationToken = default)
        {
            var page = Math.Max(1, query.Page);
            var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

            var logs = _db.AuditLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.EntityName)) logs = logs.Where(a => a.EntityName == query.EntityName);
            if (!string.IsNullOrWhiteSpace(query.EntityId)) logs = logs.Where(a => a.EntityId == query.EntityId.Trim());
            if (!string.IsNullOrWhiteSpace(query.ChangedBy)) logs = logs.Where(a => a.ChangedBy == query.ChangedBy);
            if (query.Action is { } action) logs = logs.Where(a => a.Action == action);
            if (query.FromUtc is { } from) logs = logs.Where(a => a.ChangedDate >= from);
            if (query.ToUtcExclusive is { } to) logs = logs.Where(a => a.ChangedDate < to);

            var total = await logs.CountAsync(cancellationToken);

            var items = await (
                    from a in logs
                    join actor in _db.Users on a.ChangedBy equals actor.Id into actors
                    from actor in actors.DefaultIfEmpty()
                    join target in _db.Users on a.EntityId equals target.Id into targets
                    from target in targets.DefaultIfEmpty()
                    orderby a.ChangedDate descending, a.Id descending
                    select new AuditLogEntryDto(
                        a.Id,
                        a.ChangedDate,
                        a.ChangedBy,
                        actor != null ? actor.FirstName + " " + actor.LastName : a.ChangedBy,
                        a.Action,
                        a.EntityName,
                        a.EntityId,
                        a.EntityName == "User" && target != null
                            ? target.FirstName + " " + target.LastName
                            : a.EntityName + " #" + a.EntityId,
                        a.FieldName,
                        a.OldValue,
                        a.NewValue,
                        a.IpAddress))
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

            return new PagedResult<AuditLogEntryDto>(items, page, pageSize, total);
        }
    }
}
