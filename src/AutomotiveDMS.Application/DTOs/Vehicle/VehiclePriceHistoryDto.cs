using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Vehicle
{
    public class VehiclePriceHistoryDto
    {
        public string PriceType { get; init; } = string.Empty;
        public decimal OldPrice { get; init; }
        public decimal NewPrice { get; init; }
        public string? Reason { get; init; }
        public string ChangedBy { get; init; } = string.Empty;
        public DateTime ChangedDate { get; init; }
    }
}
