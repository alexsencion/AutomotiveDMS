using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Vehicle
{
    public class VehicleSummaryDto
    {
        public int Id { get; init; }
        public string Display { get; init; } = string.Empty;
        public string Vin { get; init; } = string.Empty;
        public decimal ListPrice { get; init; }
        public string Status { get; set; } = string.Empty;
    }
}
