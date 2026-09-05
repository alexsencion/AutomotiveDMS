using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.CustomerType).HasConversion<int>().IsRequired();
            builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
            builder.Property(c => c.CompanyName).HasMaxLength(200);
            builder.Property(c => c.Cedula).HasMaxLength(11).IsFixedLength();
            builder.Property(c => c.Rnc).HasMaxLength(9).IsFixedLength();
            builder.Property(c => c.Email).HasMaxLength(256);
            builder.Property(c => c.Phone).HasMaxLength(20);
            builder.Property(c => c.Address).HasMaxLength(500);
            builder.Property(c => c.City).HasMaxLength(100);
            builder.Property(c => c.Notes).HasMaxLength(2000);

            builder.Ignore(C => C.DisplayName);

            builder.HasIndex(c => c.Cedula)
                .IsUnique()
                .HasFilter("[Cedula] IS NOT NULL")
                .HasDatabaseName("IX_Customers_Cedula");

            builder.HasIndex(c => c.Rnc)
                .IsUnique()
                .HasFilter("[Rnc] IS NOT NULL")
                .HasDatabaseName("IX_Customers_Rnc");

            builder.HasIndex(c => c.IsActive)
                .HasFilter("[IsActive] = 1")
                .HasDatabaseName("IX_Customers_IsActive");
        }
    }
}
