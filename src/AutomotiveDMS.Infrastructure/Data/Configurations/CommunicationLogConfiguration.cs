using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class CommunicationLogConfiguration : IEntityTypeConfiguration<CommunicationLog>
    {
        public void Configure(EntityTypeBuilder<CommunicationLog> builder)
        {
            builder.ToTable("CommunicationLogs");
            builder.HasKey(v => v.Id);

            builder.Property(c => c.Channel).HasConversion<int>().IsRequired();
            builder.Property(c => c.Subject).IsRequired().HasMaxLength(500);
            builder.Property(c => c.Body).IsRequired().HasMaxLength(400);
            builder.Property(c => c.ErrorMessage).HasMaxLength(1000);

            builder.HasOne(c => c.Customer)
                .WithMany(c => c.CommunicationLogs)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
