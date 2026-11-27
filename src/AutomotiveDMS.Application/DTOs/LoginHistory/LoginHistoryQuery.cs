using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.LoginHistory
{
    public sealed record LoginHistoryQuery(
        string? UserId = null,
        string? Email = null,
        LoginEventType? EventType = null,
        DateTime? FromUtc = null,
        DateTime? ToUtcExclusive = null,
        int Page = 1,
        int PageSize = 25);
}
