using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Financing
{
    public class ContractSummaryDto
    {
        public int Id { get; init; }
        public string ContractNumber { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public decimal FinancedAmount { get; init; }
        public decimal MonthlyPayment { get; init; }
        public decimal RemainingBalance { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public string VehicleDisplay { get; init; } = string.Empty;
    }
}
