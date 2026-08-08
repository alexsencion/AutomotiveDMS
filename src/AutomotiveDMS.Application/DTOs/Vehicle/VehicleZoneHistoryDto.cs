using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Vehicle
{
    public class VehicleZoneHistoryDto
    {
        public string FromZone { get; init; } = string.Empty;
        public string ToZone { get; init; } = string.Empty;
        public string? Reason { get; init; }
        public string MovedBy { get; init; } = string.Empty;
        public DateTime MovedDate { get; init; }
    }
}
