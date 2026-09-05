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
            builder.ToTable("VehicleZoneHistory");
            builder.HasKey(h => h.Id);

            builder.Property(h => h.MovedBy).IsRequired().HasMaxLength(450);
            builder.Property(h => h.Notes).HasMaxLength(500);

            builder.HasOne(h => h.Vehicle)
                .WithMany(v => v.ZoneHistory)
                .HasForeignKey(h => h.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(h => h.FromZone)
                .WithMany()
                .HasForeignKey(h => h.FromZoneId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(h => h.ToZone)
                .WithMany(z => z.ZoneHistory)
                .HasForeignKey(h => h.ToZoneId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
