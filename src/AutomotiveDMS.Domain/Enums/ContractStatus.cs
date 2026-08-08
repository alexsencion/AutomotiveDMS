using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Enums
{
    public enum ContractStatus
    {
        Active = 1,
        PaidOff = 2,
        Defaulted = 3,
        Cancelled = 4,
        Restructured = 5
    }
}
