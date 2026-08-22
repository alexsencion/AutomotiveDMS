using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class VehicleZoneHistoryConfiguration : IEntityTypeConfiguration<VehicleZoneHistory>
    {
        public void Configure(EntityTypeBuilder<VehicleZoneHistory> builder)
        {
            throw new NotImplementedException();
        }
    }
}
