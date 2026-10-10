using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Services.Email
{
    public sealed class SendGridOptions
    {
        public const string SectionName = "SendGrid";

        public bool Enabled { get; init; }
        public string ApiKey { get; init; } = string.Empty;
        public string SenderEmail { get; init; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
    }
}
