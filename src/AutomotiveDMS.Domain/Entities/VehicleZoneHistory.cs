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
        public string? Reason { get; set; }
        public string? ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedDate { get; set; }

        public Vehicle? Vehicle { get; set; }
        public Zone? FromZone { get; set; }
        public Zone? ToZone { get; set; }
    }
}
