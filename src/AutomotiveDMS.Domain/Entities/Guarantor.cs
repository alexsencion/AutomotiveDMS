using AutomotiveDMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class Guarantor : BaseEntity
    {
        public int ContractId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string Relationship { get; set; } = string.Empty;

        public FinancingContract? Contract { get; set; }
    }
}
