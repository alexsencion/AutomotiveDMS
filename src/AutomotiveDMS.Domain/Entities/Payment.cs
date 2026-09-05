using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public int ContractId { get; set; }
        public int? PaymentScheduleId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public string RecordedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public FinancingContract Contract { get; set; } = null!;
        public PaymentSchedule? PaymentSchedule { get; set; }

    }
}
