using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Financing
{
    public class PromissoryNoteDto
    {
        public int Id { get; init; }
        public string NoteNumber { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public DateTime IssueDate { get; init; }
        public DateTime DueDate { get; init; }
        public string? Notes { get; init; }
        public bool HasSignedCopy { get; init; }
    }
}
