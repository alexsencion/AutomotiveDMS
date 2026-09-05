using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class VehiclePriceHistoryConfiguration : IEntityTypeConfiguration<VehiclePriceHistory>
    {
        public void Configure(EntityTypeBuilder<VehiclePriceHistory> builder)
        {
            builder.ToTable("VehiclePriceHistory");
            builder.HasKey(h => h.Id);

            builder.Property(h => h.PriceType).IsRequired().HasMaxLength(50);
            builder.Property(h => h.OldPrice).HasColumnType("decimal(18,2)");
            builder.Property(h => h.NewPrice).HasColumnType("decimal(18,2)");
            builder.Property(h => h.ChangedBy).IsRequired().HasMaxLength(450);
            builder.Property(h => h.Notes).HasMaxLength(500);

            builder.HasOne(h => h.Vehicle)
                .WithMany(v => v.PriceHistory)
                .HasForeignKey(h => h.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
