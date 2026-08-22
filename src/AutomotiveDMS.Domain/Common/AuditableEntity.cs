using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Common
{
    public class AuditableEntity : BaseEntity
    {
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
