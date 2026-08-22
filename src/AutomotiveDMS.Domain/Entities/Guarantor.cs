using AutomotiveDMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class Guarantor : BaseEntity
    {
        public int ContractId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Cedula { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Relationship { get; set; }

        public FinancingContract? Contract { get; set; } = null!;
    }
}
