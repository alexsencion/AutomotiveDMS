using AutomotiveDMS.Application.DTOs.Financing;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Customer
{
    public class CustomerDetailDto
    {
        public int Id { get; init; }
        public string CustomerType { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PrimaryPhone { get; init; } = string.Empty;
        public string? SecondaryPhone { get; init; }
        public string Address { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string? Notes { get; init; } 
        public bool IsActive { get; init; }
        public DateTime CreatedDate { get; init; }

        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? Cedula { get; init; }

        public string? BusinessName { get; init; }
        public string? Rnc { get; init; }
        public string? ContactPerson { get; init; }

        public List<ContractSummaryDto> Contracts { get; init; } = [];
        public List<CommunicationLogDto> CommunicationLogs { get; init; } = [];
        public List<CustomerInteractionNoteDto> InteractionNotes { get; init; } = [];
        public List<DocumentSummaryDto> Documents { get; init; } = [];
    }
}
