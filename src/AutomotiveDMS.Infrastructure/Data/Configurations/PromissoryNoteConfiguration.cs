using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class PromissoryNoteConfiguration : IEntityTypeConfiguration<PromissoryNote>
    {
        public void Configure(EntityTypeBuilder<PromissoryNote> builder)
        {
            builder.ToTable("PromissoryNotes");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.NoteNumber).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.Status).HasConversion<int>();
            builder.Property(p => p.Notes).HasMaxLength(2000);

            builder.HasIndex(p => p.NoteNumber)
                .IsUnique()
                .HasDatabaseName("IX_PromissoryNotes_NoteNumber");

            builder.HasOne(p => p.Contract)
                .WithMany(c => c.PromissoryNotes)
                .HasForeignKey(p => p.Contract.Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
