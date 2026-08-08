using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Vehicle
{
    public class VehicleListDto
    {
        public int Id { get; init; }
        public string Vin { get; init; } = string.Empty;
        public string Make { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public int Year { get; init; }
        public string? Color { get; init; }
        public decimal ListPrice { get; init; }
        public string Status { get; init; } = string.Empty;
        public string ZoneName { get; init; } = string.Empty;
        public int Mileage { get; init; }
        public DateTime CreatedDate { get; init; }
    }
}
