using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Vehicle
{
    public class VehicleDetailDto
    {
        public int Id { get; init; }
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
        public string Status { get; init; } = string.Empty;
        public int ZoneId { get; init; }
        public string ZoneName { get; init; } = string.Empty;
        public string? Notes { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedDate { get; init; }
        public string? CreatedBy { get; init; }

        public List<VehicleStatusHistoryDto> StatusHistory { get; init; } = [];
        public List<VehicleZoneHistoryDto> ZoneHistory { get; init; } = [];
        public List<VehiclePriceHistoryDto> PriceHistory { get; init; } = [];
        public List<DocumentSummaryDto> Documents { get; init; } = [];
    }
}
