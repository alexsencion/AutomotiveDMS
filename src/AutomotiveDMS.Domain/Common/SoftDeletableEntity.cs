using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Common
{
    public abstract class SoftDeletableEntity : AuditableEntity
    {
        public bool IsActive { get; set; } = true;
    }
}
