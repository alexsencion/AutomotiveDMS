using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class Customer : AuditableEntity
    {
        public CustomerType CustomerType { get; set; }

        public string Email { get; set; } = string.Empty;
        public string PrimaryPhone { get; set; } = string.Empty;
        public string? SecondaryPhone { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Cedula { get; set; }

        public string? BusinessName { get; set; }
        public string? Rnc { get; set; }
        public string? ContactPerson { get; set; }

        public ICollection<FinancingContract> Contracts { get; set; } = new List<FinancingContract>();
        public ICollection<CommunicationLog> CommunicationLog { get; set; } = new List<CommunicationLog>();
        public ICollection<CustomerInteractionNote> InteractionNotes { get; set; } = new List<CustomerInteractionNote>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();

    }
}
