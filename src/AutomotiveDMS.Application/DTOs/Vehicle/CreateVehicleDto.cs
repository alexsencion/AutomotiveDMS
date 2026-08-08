using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Vehicle
{
    public class CreateVehicleDto
    {
        public string Vin { get; init; } = string.Empty;
        public string Make { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public int Year { get; init; }
        public string? Color { get; init; }
        public string? Trim { get; init; }
        public decimal PurchasePrice { get; init; }
        public decimal ListPrice { get; init; }
        public int Mileage { get; init; }
        public string? EngineType { get; init; }
        public string? Transmission { get; init; }
        public int ZoneId { get; init; }
        public string? Notes { get; init; }
    }
}
