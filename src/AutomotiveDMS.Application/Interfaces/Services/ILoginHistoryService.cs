using AutomotiveDMS.Application.Common.Models;
using AutomotiveDMS.Application.DTOs.LoginHistory;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.Interfaces.Services
{
    public interface ILoginHistoryService
    {
        Task RecordAsync(LoginEventType eventType, string email, string? userId,
            CancellationToken cancellationToken = default);

        Task<PagedResult<LoginEventDto>> SearchAsync(LoginHistoryQuery query, 
            CancellationToken cancellationToken = default);
    }
}
