using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Financing
{
    public class PaymentScheduleDto
    {
        public int InstallmentNumber { get; init; }
        public DateTime DueDate { get; init; }
        public decimal TotalDue { get; init; }
        public decimal PrincipalDue { get; init; }
        public decimal InterestDue { get; init; }
        public decimal RemainingBalance { get; init; }
        public bool IsPaid { get; init; }
        public DateTime? PaidDate { get; init; }
    }
}
