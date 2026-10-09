using AutomotiveDMS.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task<bool> SendAsync(EmailMessage message, CancellationToken cancellationToken);
    }
}
