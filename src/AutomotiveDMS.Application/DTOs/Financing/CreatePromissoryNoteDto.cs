using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Financing
{
    public class CreatePromissoryNoteDto
    {
        public int ContractId { get; init; }
        public decimal Amount { get; init; }
        public DateTime IssueDate { get; init; }
        public DateTime DueDate { get; init; }
        public string? Notes { get; init; }
    }
}
