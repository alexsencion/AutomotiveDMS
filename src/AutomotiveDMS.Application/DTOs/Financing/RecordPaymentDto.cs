using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Financing
{
    public class RecordPaymentDto
    {
        public int ContractId { get; init; }
        public decimal Amount { get; init; }
        public string PaymentMethod { get; init; } = string.Empty;
        public DateTime PaymentDate { get; init; }
        public string ReceiptNumber { get; init; } = string.Empty;
        public int? PaymentScheduledId { get; init; }
        public string? Notes { get; init; }
    }
}
