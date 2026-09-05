using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class VehicleStatusHistoryConfiguration : IEntityTypeConfiguration<VehicleStatusHistory>
    {
        public void Configure(EntityTypeBuilder<VehicleStatusHistory> builder)
        {
            builder.ToTable("VehicleStatusHistory");
            builder.HasKey(h => h.Id);

            builder.Property(h => h.ChangedBy).IsRequired().HasMaxLength(450);
            builder.Property(h => h.Notes).HasMaxLength(500);
            builder.Property(h => h.OldStatus).HasConversion<int>();
            builder.Property(h => h.NewStatus).HasConversion<int>();

            builder.HasOne(h => h.Vehicle)
                .WithMany(v => v.StatusHistory)
                .HasForeignKey(h => h.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
