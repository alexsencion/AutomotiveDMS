using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Audit
{
    public sealed record AuditLogQuery(
        string? EntityName = null,
        string? EntityId = null,
        string? ChangedBy = null,
        AuditAction? Action = null,
        DateTime? FromUtc = null,
        DateTime? ToUtcExclusive = null,
        int Page = 1,
        int PageSize = 25);
}
