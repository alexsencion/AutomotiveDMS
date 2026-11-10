using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Audit
{
    public sealed record AuditLogEntryDto(
        long Id,
        DateTime ChangedDateUtc,
        string ChangedBy,
        string ChangedByName,
        AuditAction Action,
        string EntityName,
        string EntityId,
        string EntityDisplay,
        string? FieldName,
        string? OldValue,
        string? NewValue,
        string? IpAddres);
}
