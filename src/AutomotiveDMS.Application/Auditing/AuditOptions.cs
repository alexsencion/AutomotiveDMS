using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.Auditing
{
    public sealed class AuditOptions
    {
        public static readonly FrozenSet<string> DefaultExcludedProperties = new[]
        {
            "CreatedBy", "CreatedDate", "ModifiedBy", "ModifiedDate",
            "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
            "NormalizedEmail", "NormalizedUserName",
            "LastLoginDate", "AccessFailedCount", "LockoutEnd"
        }.ToFrozenSet(StringComparer.Ordinal);

        public IReadOnlySet<string> ExcludedProperties { get; init; } = DefaultExcludedProperties;

        public string SoftDeleteProperty { get; init; } = "IsActive";

        public int MaxValueLength { get; set; } = 4000;
    }
}
