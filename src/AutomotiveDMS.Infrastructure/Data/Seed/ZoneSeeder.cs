using AutomotiveDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Seed
{
    public static class ZoneSeeder
    {
        private static readonly (string Name, string Description)[] DefaultZones =
        [
            ("Dealership",   "Vehicles physically at the dealership lot or showroom"),
            ("Repair Shop",  "Vehicles at external repair facility for servicing"),
            ("In Transit",   "Vehicles being imported or in transit to the dealership")
        ];

        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

            foreach (var (name, description) in DefaultZones)
            {
                var exists = await context.Zones
                    .AnyAsync(z => z.Name == name);

                if (exists)
                {
                    logger.LogDebug("Zone '{Zone}' already exists - skipping", name);
                    continue;
                }

                context.Zones.Add(new Zone
                {
                    Name = name,
                    Description = description,
                    IsActive = true,
                    CreatedBy = "SYSTEM",
                    CreatedDate = DateTime.UtcNow,
                });

                logger.LogInformation("Queued zone '{Zone}' for creation", name);
            }

            var saved = await context.SaveChangesAsync();

            if (saved > 0)
            {
                logger.LogInformation("Saved {Count} zone(s) to database", saved);
            }
        }
    }
}
