using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Financing
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public int? InstallmentNumber { get; set; }
        public string? Notes { get; set; }
        public string RecordedBy { get; set; } = string.Empty;
    }
}
