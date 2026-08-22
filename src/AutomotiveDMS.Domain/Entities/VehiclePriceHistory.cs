using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class VehiclePriceHistory : BaseEntity
    {
        public int VehicleId { get; set; }
        public PriceType PriceType { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedDate { get; set; }
        public string? Notes { get; set; }

        public Vehicle Vehicle { get; set; } = null!;
    }
}
