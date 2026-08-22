using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.ToTable("Documents");
            builder.HasKey(p => p.Id);

            builder.Property(d => d.DocumentType).HasConversion<int>().IsRequired();
            builder.Property(d => d.FileName).IsRequired().HasMaxLength(260);
            builder.Property(d => d.BlobPath).IsRequired().HasMaxLength(1024);
            builder.Property(d => d.ContentType).IsRequired().HasMaxLength(100);
            builder.Property(d => d.UploadedBy).IsRequired().HasMaxLength(450);

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Documents_AtLeastOneEntity",
                "[CustomerId] IS NOT NULL OR [VehicleId] IS NOT NULL OR " +
                "[FinancingContractId] IS NOT NULL OR [PromissoryNoteId] IS NOT NULL"));

            builder.HasOne(d => d.Customer)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasOne(d => d.Vehicle)
                .WithMany(v => v.Documents)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasOne(d => d.FinancingContract)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.FinancingContractId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasOne(d => d.PromissoryNote)
                .WithMany(v => v.Documents)
                .HasForeignKey(d => d.PromissoryNoteId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}
