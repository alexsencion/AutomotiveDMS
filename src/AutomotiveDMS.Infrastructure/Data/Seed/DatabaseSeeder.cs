using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Seed
{
    public class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();

            try
            {
                logger.LogInformation("Starting database seeding...");

                
                logger.LogInformation("Seeding roles...");
                await RoleSeeder.SeedAsync(services);

                logger.LogInformation("Seeding admin user...");
                await UserSeeder.SeedAsync(services);

                logger.LogInformation("Seeding zones...");
                await ZoneSeeder.SeedAsync(services);

                logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database seeding failed");
                throw;
            }
        }
    }
}
