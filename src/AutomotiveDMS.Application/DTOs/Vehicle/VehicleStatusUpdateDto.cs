using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Vehicle
{
    public class VehicleStatusUpdateDto
    {
        public int VehicleId { get; init; }
        public string NewStatus { get; init; } = string.Empty;
        public string? Reason { get; init; }
    }
}
