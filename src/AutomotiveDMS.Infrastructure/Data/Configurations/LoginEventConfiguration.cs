using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class LoginEventConfiguration : IEntityTypeConfiguration<LoginEvent>
    {
        public void Configure(EntityTypeBuilder<LoginEvent> builder)
        {
            builder.ToTable("LoginEvents");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.UserId).HasMaxLength(450);
            builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
            builder.Property(e => e.EventType).HasConversion<int>().IsRequired();
            builder.Property(e => e.OccurredAt).HasColumnType("datetime2");
            builder.Property(e => e.IpAddress).HasMaxLength(45);
            builder.Property(e => e.UserAgent).HasMaxLength(512);

            builder.HasIndex(e => e.OccurredAt).HasDatabaseName("IX_LoginEvents_OccurredAt");

            builder.HasIndex(e => new { e.UserId, e.OccurredAt }).HasDatabaseName("IX_LoginEvents_User_Date");
        }
    }
}
