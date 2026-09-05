using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class PaymentScheduleConfiguration : IEntityTypeConfiguration<PaymentSchedule>
    {
        public void Configure(EntityTypeBuilder<PaymentSchedule> builder)
        {
            builder.ToTable("PaymentSchedules");
            builder.HasKey(g => g.Id);

            builder.Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.PrincipalAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.InterestAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.Balance).HasColumnType("decimal(18,2)");

            builder.HasIndex(p => new { p.ContractId, p.InstallmentNumber })
                .IsUnique()
                .HasDatabaseName("IX_PaymentSchedules_ContractId_InstallmentNumber");

            builder.HasOne(p => p.Contract)
                .WithMany(c => c.PaymentSchedules)
                .HasForeignKey(p => p.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
