using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class FinancingContract : AuditableEntity
    {
        public string ContractNumber { get; set; } = string.Empty;
        public ContractStatus Status { get; set; } = ContractStatus.Active;

        public int CustomerId { get; set; }
        public int VehicleId { get; set; }

        public decimal PrincipalAmount { get; set; }
        public decimal DownPayment { get; set; }
        public decimal FinancedAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public decimal MonthlyPayment { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string? Notes { get; set; }

        public Customer? Customer { get; set; }
        public Vehicle? Vehicle { get; set; }
        public ICollection<Guarantor> Guarantors { get; set; } = new List<Guarantor>();
        public ICollection<PaymentSchedule> PaymentSchedules { get; set; } = new List<PaymentSchedule>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<PromissoryNote> PromissoryNotes { get; set; } = new List<PromissoryNote>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();

    }
}
