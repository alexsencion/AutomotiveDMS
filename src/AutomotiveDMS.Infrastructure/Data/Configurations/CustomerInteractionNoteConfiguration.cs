using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class CustomerInteractionNoteConfiguration : IEntityTypeConfiguration<CustomerInteractionNote>
    {
        public void Configure(EntityTypeBuilder<CustomerInteractionNote> builder)
        {
            builder.ToTable("CustomerInteractionNotes");
            builder.HasKey(v => v.Id);

            builder.Property(n => n.Note).IsRequired().HasMaxLength(2000);
            builder.Property(n => n.CreatedBy).IsRequired().HasMaxLength(450);

            builder.HasOne(n => n.Customer)
                .WithMany(c => c.InteractionNotes)
                .HasForeignKey(n => n.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
