using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Dashboard
{
    public class InventoryOverviewDto
    {
        public int TotalVehicles { get; init; }
        public int AvailableVehicles { get; init; }
        public int SoldThisMonth { get; init; }
        public int InRepair { get; init; }
        public decimal TotalInventoryValue { get; init; }
        public decimal AverageDaysOnLot { get; init; }
    }
}
