using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
    {
        public void Configure(EntityTypeBuilder<Zone> builder)
        {
            builder.ToTable("Zones");
            builder.HasKey(z => z.Id);

            builder.Property(z => z.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(z => z.Description)
                .HasMaxLength(500);

            builder.HasIndex(z => z.Name)
                .IsUnique()
                .HasDatabaseName("IX_Zones_Name");
        }
    }
}
