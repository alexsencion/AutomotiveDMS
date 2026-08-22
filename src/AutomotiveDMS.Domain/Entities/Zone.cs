using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class Zone : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Vehicle> Vehicles { get; set; } = [];
        public ICollection<VehicleZoneHistory> ZoneHistory { get; set; } = [];
    }
}
