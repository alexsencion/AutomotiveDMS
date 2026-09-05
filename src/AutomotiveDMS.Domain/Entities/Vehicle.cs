using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class Vehicle : SoftDeletableEntity
    {
        public string VIN { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string? Color { get; set; }
        public string? Trim { get; set; }
        public int? Mileage { get; set; }
        public VehicleStatus Status { get; set; } = VehicleStatus.Available;
        public decimal PurchasePrice { get; set; }
        public decimal ListPrice { get; set; }
        public decimal? SalesPrice { get; set; }
        public string? Notes { get; set; }

        public int ZoneId { get; set; }
        public Zone Zone { get; set; } = null!;
        public ICollection<VehicleStatusHistory> StatusHistory { get; set; } = [];
        public ICollection<VehicleZoneHistory> ZoneHistory { get; set; } = [];
        public ICollection<VehiclePriceHistory> PriceHistory { get; set; } = [];
        public ICollection<Document> Documents { get; set; } = [];
        public ICollection<FinancingContract> Contracts { get; set; } = [];

    }
}
