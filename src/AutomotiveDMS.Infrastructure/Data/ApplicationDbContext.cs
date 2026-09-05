using AutomotiveDMS.Domain.Entities;
using AutomotiveDMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Zone> Zones { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleStatusHistory> VehicleStatusHistory { get; set; }
        public DbSet<VehicleZoneHistory> VehicleZoneHistory { get; set; }
        public DbSet<VehiclePriceHistory> VehiclePriceHistory { get; set; }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<CommunicationLog> CommunicationLogs { get; set; }
        public DbSet<CustomerInteractionNote> CustomerInteractionNotes { get; set; }

        public DbSet<FinancingContract> FinancingContracts { get; set; }
        public DbSet<Guarantor> Guarantors { get; set; }
        public DbSet<PaymentSchedule> PaymentSchedules { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PromissoryNote> PromissoryNotes { get; set; }

        public DbSet<Document> Documents { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<Domain.Common.AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = now;
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedDate = now;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
