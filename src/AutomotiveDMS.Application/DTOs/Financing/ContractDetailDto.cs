using AutomotiveDMS.Application.DTOs.Customer;
using AutomotiveDMS.Application.DTOs.Vehicle;
using AutomotiveDMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Financing
{
    public class ContractDetailDto
    {
        public int Id { get; init; }
        public string ContractNumber { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public decimal PrincipalAmount { get; init; }
        public decimal DownPayment{ get; init; }
        public decimal FinancedAmount { get; init; }
        public decimal InterestRate { get; init; }
        public int TermMonths { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public decimal MonthlyPayment { get; init; }
        public decimal TotalPaid { get; init; }
        public decimal RemainingBalance { get; init; }
        public string? Notes { get; init; }

        public CustomerSummaryDto Customer { get; init; } = new();
        public VehicleSummaryDto Vehicle { get; init; } = new();

        public List<GuarantorDto> Guarantors { get; init; } = [];
        public List<PaymentScheduleDto> PaymentSchedule { get; init; } = [];
        public List<PaymentDto> Payments { get; init; } = [];
        public List<PromissoryNoteDto> PromissoryNotes { get; init; } = [];
        public List<DocumentSummaryDto> Documents { get; init; } = [];
    }
}
