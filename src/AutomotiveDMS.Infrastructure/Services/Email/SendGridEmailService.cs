using AutomotiveDMS.Application.Common.Models;
using AutomotiveDMS.Application.Interfaces.Repositories;
using AutomotiveDMS.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Services.Email
{
    public sealed class SendGridEmailService : IEmailService
    {
        private readonly ISendGridClient _client;
        private readonly SendGridOptions _options;
        private readonly ILogger<SendGridEmailService> _logger;

        public SendGridEmailService(
            ISendGridClient client,
            IOptions<SendGridOptions> options,
            ILogger<SendGridEmailService> logger)
        {
            _client = client;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<bool> SendAsync(
            EmailMessage message, CancellationToken cancellationToken = default)
        {
            var mail = MailHelper.CreateSingleEmail(
                from: new EmailAddress(_options.SenderEmail, _options.SenderName),
                to: new EmailAddress(message.To, message.ToName),
                subject: message.Subject,
                plainTextContent: message.PlainTextBody,
                htmlContent: message.HtmlBody);

            mail.SetClickTracking(enable: false, enableText: false);

            try
            {
                var response = await _client.SendEmailAsync(mail, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Email '{Subject}' accepted by SendGrid for {Recipient}",
                        message.Subject, message.To);
                    return true;
                }

                var body = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "SendGrid rejected email '{Subject}' ({StatusCode}): {Body}",
                    message.Subject, (int)response.StatusCode, body);
                return false;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex,
                    "SendGrid call failed for email '{Subject}'", message.Subject);
                return false;
            }
        }
    }
}
