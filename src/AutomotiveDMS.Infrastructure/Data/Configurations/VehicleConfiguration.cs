
using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("Vehicles");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.VIN)
                .IsRequired()
                .HasMaxLength(17)
                .IsFixedLength();

            builder.Property(v => v.Make)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(v => v.Model)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(v => v.Color)
                .HasMaxLength(50);

            builder.Property(v => v.Trim)
                .HasMaxLength(100);

            builder.Property(v => v.Notes)
                .HasMaxLength(2000);

            builder.Property(v => v.PurchasePrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(v => v.ListPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(v => v.SalesPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(v => v.Status)
                .HasConversion<int>();

            builder.HasIndex(v => v.VIN)
                .IsUnique()
                .HasDatabaseName("IX_Vehicles_VIN");

            builder.HasIndex(v => v.Status)
                .HasDatabaseName("IX_Vehicles_Status");

            builder.HasIndex(v => v.IsActive)
                .HasFilter("[IsActive] = 1 ")
                .HasDatabaseName("IX_Vehicles_IsActive");

            builder.HasOne(v => v.Zone)
                .WithMany(z => z.Vehicles)
                .HasForeignKey(v => v.ZoneId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
