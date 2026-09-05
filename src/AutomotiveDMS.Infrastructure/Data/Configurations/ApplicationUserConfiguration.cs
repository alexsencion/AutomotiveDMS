using AutomotiveDMS.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Ignore(u => u.FullName);

            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(u => u.CreatedDate)
                .IsRequired()
                .HasColumnType("datetime2");

            builder.Property(u => u.LastLoginDate)
                .IsRequired(false)
                .HasColumnType("datetime2");

            builder.HasIndex(u => u.IsActive)
                .HasFilter("[IsActive] = 1 ")
                .HasDatabaseName("IX_AspNetUsers_IsActive");
        }
    }
}
