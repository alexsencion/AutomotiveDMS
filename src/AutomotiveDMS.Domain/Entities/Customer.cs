using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class Customer : SoftDeletableEntity
    {
        public CustomerType CustomerType { get; set; }
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? Cedula { get; set; }
        public string? Rnc { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Notes { get; set; }

        public string DisplayName => CustomerType == CustomerType.Business && !string.IsNullOrWhiteSpace(CompanyName)
            ? CompanyName
            : $"{FirstName} {LastName}".Trim();

        public ICollection<Document> Documents { get; set; } = [];
        public ICollection<CommunicationLog> CommunicationLogs { get; set; } = [];
        public ICollection<CustomerInteractionNote> InteractionNotes { get; set; } = [];
        public ICollection<FinancingContract> Contracts { get; set; } = [];

    }
}
