using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.LoginHistory
{
    public sealed record LoginEventDto(
        int Id,
        DateTime OccurredAtUtc,
        string? UserId,
        string? UserName,
        string Email,
        LoginEventType EventType,
        string? IpAddress,
        string? UserAgent);
}
