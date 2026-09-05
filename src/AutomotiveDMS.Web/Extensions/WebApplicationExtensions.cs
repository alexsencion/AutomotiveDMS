using AutomotiveDMS.Infrastructure.Data;
using AutomotiveDMS.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace AutomotiveDMS.Web.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task InitialiseDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Initialising database...");

                var context = services.GetRequiredService<ApplicationDbContext>();

                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    logger.LogInformation("Appliying pending migrations...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully");
                }
                else
                {
                    logger.LogInformation("No pending migrations found");
                }

                logger.LogInformation("Seeding reference data...");
                await DatabaseSeeder.SeedAsync(services);
                logger.LogInformation("Database Initialisation complete");
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "An error occurred during database initialisation. " +
                    "The application will not start.");
                throw;
            }
        }
    }
}
