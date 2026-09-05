using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class FinancingContract : AuditableEntity
    {
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal PrincipalAmount { get; set; }
        public decimal DownPayment { get; set; }
        public decimal FinancedAmount { get; set; }

        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public decimal MonthlyPayment { get; set; }


        public ContractStatus Status { get; set; } = ContractStatus.Active;
        public string? Notes { get; set; }

        public Customer Customer { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public ICollection<Guarantor> Guarantors { get; set; } = [];
        public ICollection<PaymentSchedule> PaymentSchedules { get; set; } = [];
        public ICollection<Payment> Payments { get; set; } = [];
        public ICollection<PromissoryNote> PromissoryNotes { get; set; } = [];
        public ICollection<Document> Documents { get; set; } = [];

    }
}
