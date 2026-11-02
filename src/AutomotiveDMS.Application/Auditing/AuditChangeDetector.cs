using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AutomotiveDMS.Application.Auditing
{
    public class AuditChangeDetector
    {
        private readonly AuditOptions _options;
        private const string Iso8601Format = "yyyy-MM-dd'T'HH:mm:ss.fffffffK";

        public AuditChangeDetector(AuditOptions? options = null) =>
            _options = options ?? new AuditOptions();

        public IReadOnlyList<AuditChange> Detect(EntitySnapshot snapshot)
        {
            ArgumentNullException.ThrowIfNull(snapshot);

            return snapshot.State switch
            {
                EntityChangeState.Added => [Summary(snapshot, AuditAction.Created)],
                EntityChangeState.Deleted => [Summary(snapshot, AuditAction.Deleted)],
                EntityChangeState.Modified => DetectModifications(snapshot),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(snapshot), snapshot.State, "Unknown entity change state.")
            };
        }

        private List<AuditChange> DetectModifications(EntitySnapshot snapshot)
        {
            var changes = new List<AuditChange>();

            foreach (var property in snapshot.Properties)
            {
                if (_options.ExcludedProperties.Contains(property.Name) ||
                    AreEqual(property.OriginalValue, property.CurrentValue))
                {
                    continue;
                }

                changes.Add(new AuditChange(
                    snapshot.EntityName,
                    snapshot.EntityId,
                    ResolveAction(property),
                    property.Name,
                    Truncate(Format(property.OriginalValue)),
                    Truncate(Format(property.CurrentValue))));
            }

            return changes;
        }

        private AuditAction ResolveAction(PropertySnapshot property) =>
            property.Name == _options.SoftDeleteProperty &&
            property.OriginalValue is bool before &&
            property.CurrentValue is bool
                ? (before ? AuditAction.Deleted : AuditAction.Restored)
                : AuditAction.Updated;

        private static AuditChange Summary(EntitySnapshot snapshot, AuditAction action) =>
            new(snapshot.EntityName, snapshot.EntityId, action, null, null, null);

        private static bool AreEqual(object? original, object? current) =>
            Equals(NormalizeEmpty(original), NormalizeEmpty(current));

        private static object? NormalizeEmpty(object? value) =>
            value is string { Length: 0 } ? null : value;

        private static string? Format(object? value) => value switch
        {
            null => null,
            string s => s,
            decimal d => d.ToString("0.00", CultureInfo.InvariantCulture),
            DateTime dt => dt.ToString(Iso8601Format, CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString(Iso8601Format, CultureInfo.InvariantCulture),
            bool b => b ? "true" : "false",
            Enum e => e.ToString(),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };

        private string? Truncate(string? value) =>
            value is null || value.Length <= _options.MaxValueLength
                ? value
                : string.Concat(value.AsSpan(0, _options.MaxValueLength - 1), "\u2026");
    }
}
