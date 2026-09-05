using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Vehicle
{
    public class VehicleZoneMoveDto
    {
        public int VehicleId { get; init; }
        public int NewZoneId { get; init; }
        public string? Reason { get; init; }
    }
}
