using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class VehicleStatusHistory : BaseEntity
    {
        public int VehicleId { get; set; }
        public VehicleStatus OldStatus { get; set; }
        public VehicleStatus NewStatus { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedDate { get; set; }
        public string? Notes { get; set; }

        public Vehicle Vehicle { get; set; } = null!;
    }
}
