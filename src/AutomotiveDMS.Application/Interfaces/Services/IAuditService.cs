using AutomotiveDMS.Application.Common.Models;
using AutomotiveDMS.Application.DTOs.Audit;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.Interfaces.Services
{
    public interface IAuditService
    {
        Task<PagedResult<AuditLogEntryDto>> SearchAsync(AuditLogQuery query, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<string>> GetEntityNamesAsync(CancellationToken cancellationToken = default);
    }
}
