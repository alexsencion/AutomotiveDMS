using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class PromissoryNote : AuditableEntity
    {
        public int ContractId { get; set; }
        public string NoteNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public PromissoryNoteStatus Status { get; set; } = PromissoryNoteStatus.Draft;
        public string? Notes { get; set; }

        public FinancingContract Contract { get; set; } = null!;
        public ICollection<Document> Documents { get; set; } = [];
    }
}
