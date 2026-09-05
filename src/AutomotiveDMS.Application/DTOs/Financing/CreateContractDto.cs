using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Financing
{
    public class CreateContractDto
    {
        public int CustomerId { get; init; }
        public int VehicleId { get; init; }
        public decimal PrincipalAmount { get; init; }
        public decimal DownPayment { get; init; }
        public decimal InterestRate { get; init; }
        public int TermMonths { get; init; }
        public DateTime StartDate { get; init; }
        public string? Notes { get; init; }
    }
}
