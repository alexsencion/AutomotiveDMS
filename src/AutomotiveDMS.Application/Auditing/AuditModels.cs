using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.Auditing
{
    public enum EntityChangeState
    {
        Added,
        Modified,
        Deleted
    }

    public sealed record PropertySnapshot(string Name, object? OriginalValue, object? CurrentValue);

    public sealed record EntitySnapshot(
        string EntityName,
        string EntityId,
        EntityChangeState State,
        IReadOnlyList<PropertySnapshot> Properties);

    public sealed record AuditChange(
        string EntityName,
        string EntityId,
        AuditAction Action,
        string? FieldName,
        string? OldValue,
        string? NewValue);
}
