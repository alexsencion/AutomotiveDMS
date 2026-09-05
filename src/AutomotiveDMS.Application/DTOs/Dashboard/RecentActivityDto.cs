using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Dashboard
{
    public class RecentActivityDto
    {
        public string ActivityType { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string PerformedBy { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
        public string? EntityLink { get; init; }
    }
}
