using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class VehicleZoneHistory : BaseEntity
    {
        public int VehicleId { get; set; }
        public int FromZoneId { get; set; }
        public int ToZoneId { get; set; }
        public string? MovedBy { get; set; } = string.Empty;
        public DateTime MovedDate { get; set; }
        public string? Notes { get; set; }
        public Vehicle Vehicle { get; set; } = null!;
        public Zone FromZone { get; set; } = null!;
        public Zone?ToZone { get; set; } = null!;
    }
}
