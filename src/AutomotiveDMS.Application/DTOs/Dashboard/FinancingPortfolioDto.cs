using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Dashboard
{
    public class FinancingPortfolioDto
    {
        public int ActiveContracts { get; init; }
        public decimal TotalPortfolioValue { get; init; }
        public decimal TotalCollectedThisMonth { get; init; }
        public int OverdueContracts { get; init; }
        public decimal OverdueAmount { get; init; }
        public decimal CollectionRate { get; init; }
    }
}
