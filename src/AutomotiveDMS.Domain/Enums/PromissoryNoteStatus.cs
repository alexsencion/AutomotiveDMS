using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Enums
{
    public enum PromissoryNoteStatus
    {
        Draft = 1,
        Issued = 2,
        Signed = 3,
        Paid = 4,
        Defaulted = 5,
        Cancelled = 6
    }
}
