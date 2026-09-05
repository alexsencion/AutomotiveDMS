using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Vehicle
{
    public class VehiclePriceUpdateDto
    {
        public int VehicleId { get; init; }
        public string PriceType { get; init; } = string.Empty;
        public decimal NewZoneId { get; init; }
        public string? Reason { get; init; }
    }
}
