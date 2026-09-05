using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.PaymentMethod).HasConversion<int>();
            builder.Property(p => p.ReceiptNumber).IsRequired().HasMaxLength(50);
            builder.Property(p => p.RecordedBy).IsRequired().HasMaxLength(450);
            builder.Property(p => p.Notes).HasMaxLength(1000);

            builder.HasIndex(p => p.ReceiptNumber)
                .IsUnique()
                .HasDatabaseName("IX_Payments_ReceiptNumber");

            builder.HasOne(p => p.Contract)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.ContractId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.PaymentSchedule)
                .WithMany()
                .HasForeignKey(p => p.PaymentScheduleId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}
