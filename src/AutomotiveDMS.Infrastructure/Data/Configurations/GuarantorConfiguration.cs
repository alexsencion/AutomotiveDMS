using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class GuarantorConfiguration : IEntityTypeConfiguration<Guarantor>
    {
        public void Configure(EntityTypeBuilder<Guarantor> builder)
        {
            builder.ToTable("Guarantors");
            builder.HasKey(g => g.Id);

            builder.Property(g => g.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(g => g.LastName).IsRequired().HasMaxLength(100);
            builder.Property(g => g.Cedula).HasMaxLength(11).IsFixedLength();
            builder.Property(g => g.Phone).HasMaxLength(20);
            builder.Property(g => g.Email).HasMaxLength(256);
            builder.Property(g => g.Address).HasMaxLength(500);
            builder.Property(g => g.Relationship).HasMaxLength(100);

            builder.HasOne(g => g.Contract)
                .WithMany(c => c.Guarantors)
                .HasForeignKey(g => g.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
