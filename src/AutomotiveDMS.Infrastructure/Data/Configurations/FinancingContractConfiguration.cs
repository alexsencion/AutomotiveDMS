using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    internal class FinancingContractConfiguration : IEntityTypeConfiguration<FinancingContract>
    {
        public void Configure(EntityTypeBuilder<FinancingContract> builder)
        {
            builder.ToTable("FinancingContracts");
            builder.HasKey(f => f.Id);

            builder.Property(f => f.ContractNumber).IsRequired().HasMaxLength(50);
            builder.Property(f => f.PrincipalAmount).HasColumnType("decimal(18,2)");
            builder.Property(f => f.DownPayment).HasColumnType("decimal(18,2)");
            builder.Property(f => f.InterestRate).HasColumnType("decimal(18,2)");
            builder.Property(f => f.MonthlyPayment).HasColumnType("decimal(18,2)");
            builder.Property(f => f.Notes).HasMaxLength(2000);
            builder.Property(f => f.Status).HasConversion<int>();

            builder.Property(f => f.FinancedAmount)
                .HasColumnType("decimal(18,2)")
                .HasComputedColumnSql("[PrincipalAmount] - [DownPayment]", stored: true);

            builder.HasIndex(f => f.ContractNumber)
                .IsUnique()
                .HasDatabaseName("IX_FinancingContracts_ContractNumber");

            builder.HasOne(f => f.Customer)
                .WithMany(c => c.Contracts)
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Vehicle)
                .WithMany(v => v.Contracts)
                .HasForeignKey(f => f.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
