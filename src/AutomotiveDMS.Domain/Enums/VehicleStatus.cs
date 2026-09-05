using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Enums
{
    public enum VehicleStatus
    {
        Available = 1,
        Reserved = 2,
        Sold = 3,
        InRepair = 4,
        InTransit = 5,
        Inactive = 6,
    }
}
