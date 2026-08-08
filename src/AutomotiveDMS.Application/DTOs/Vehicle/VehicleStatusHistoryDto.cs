using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Vehicle
{
    public class VehicleStatusHistoryDto
    {
        public string OldStatus { get; init; } = string.Empty;
        public string NewStatus { get; init; } = string.Empty;
        public string? Reason { get; init; }
        public string ChangedBy { get; init; } = string.Empty;
        public DateTime ChangedDate { get; init; }
    }
}
