using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class PromissoryNote : BaseEntity
    {
        public int ContractId { get; set; }
        public string NoteNumber { get; set; } = string.Empty;
        public PromissoryNoteStatus Status { get; set; } = PromissoryNoteStatus.Draft;
        public decimal Amount { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public string? Notes { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        public FinancingContract? Contract { get; set; }
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
