using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public InventoryOverviewDto Inventory { get; init; } = new();
        public FinancingPortfolioDto Financing { get; init; } = new();
        public List<RecentActivityDto> RecentActivity { get; init; } = [];
        public List<AgingAlertDto> AgingAlerts{ get; init; } = [];
    }
}
