using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class CommunicationLog : BaseEntity
    {
        public int CustomerId { get; set; }
        public CommunicationChannel Channel { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ExternalId { get; set; }
        public DateTime SentDate { get; set; }

        public Customer? Customer { get; set; }
    }
}
