using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class LoginEvent : BaseEntity
    {
        public string? UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public LoginEventType EventType { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
