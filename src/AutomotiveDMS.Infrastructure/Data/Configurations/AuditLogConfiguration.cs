using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Action).HasConversion<int>().IsRequired();
            builder.Property(a => a.FieldName).HasMaxLength(100);
            builder.Property(a => a.OldValue).HasMaxLength(4000);
            builder.Property(a => a.NewValue).HasMaxLength(4000);
            builder.Property(a => a.ChangedBy).IsRequired().HasMaxLength(450);
            builder.Property(a => a.IpAddress).HasMaxLength(45);

            builder.HasIndex(a => new { a.EntityName, a.EntityId, a.ChangedDate })
                .HasDatabaseName("IX_AuditLogs_Entity_Date");

        }
    }
}
