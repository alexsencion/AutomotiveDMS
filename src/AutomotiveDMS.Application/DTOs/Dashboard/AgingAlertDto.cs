using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Dashboard
{
    public class AgingAlertDto
    {
        public string AlertType { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Severity { get; init; } = string.Empty;
        public string? EntityLink { get; init; }
    }
}
