using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class Vehicle : AuditableEntity
    {
        public string Vin { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string? Color { get; set; }
        public string? Trim { get; set; }

        public int Mileage { get; set; }
        public string? EngineType { get; set; }
        public string? Transmission { get; set; }

        public decimal PurchasePrice { get; set; }
        public decimal ListPrice { get; set; }

        public VehicleStatus Status { get; set; } = VehicleStatus.Available;
        public int ZoneId { get; set; }
        public string? Notes { get; set; }

        public Zone? Zone { get; set; }
        public ICollection<VehicleStatusHistory> StatusHistory { get; set; } = new List<VehicleStatusHistory>();
        public ICollection<VehicleZoneHistory> ZoneHistory { get; set; } = new List<VehicleZoneHistory>();
        public ICollection<VehiclePriceHistory> PriceHistory { get; set; } = new List<VehiclePriceHistory>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<FinancingContract> Contracts { get; set; } = new List<FinancingContract>();

    }
}
