using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Customer
{
    public class CommunicationLogDto
    {
        public int Id { get; init; }
        public string Channel { get; init; } = string.Empty;
        public string Subject { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime SentDate { get; init; }
    }
}
