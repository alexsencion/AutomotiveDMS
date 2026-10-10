using AutomotiveDMS.Application.Common.Models;
using AutomotiveDMS.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Services.Email
{
    public sealed class LoggingEmailService : IEmailService
    {
        private readonly ILogger<LoggingEmailService> _logger;

        public LoggingEmailService(ILogger<LoggingEmailService> logger) => _logger = logger;

        public Task<bool> SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            _logger.LogWarning(
                "[EMAIL NOT SENT - SendGrid disabled] {To}: | Subject: {Subject}{NewLine}{Body}",
                message.To, message.Subject, Environment.NewLine, message.PlainTextBody);

            return Task.FromResult(true);
        }
    }
}
