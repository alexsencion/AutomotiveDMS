using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class CustomerInteractionNote : BaseEntity
    {
        public int CustomerId { get; set; }
        public string Note { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        public Customer Customer { get; set; } = null!;
    }
}
