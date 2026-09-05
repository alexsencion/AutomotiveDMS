using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Seed
{
    public static class RoleSeeder
    {
        public static readonly string[] Roles =
        [
            "Admin",
            "Manager",
            "Secretary"
        ];

        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

            foreach (var roleName in Roles)
            {
                if (await roleManager.RoleExistsAsync(roleName))
                {
                    logger.LogDebug("Role '{Role}' already exists - skipping", roleName);
                    continue;
                }

                var result = await roleManager.CreateAsync(new IdentityRole(roleName));

                if (result.Succeeded)
                {
                    logger.LogInformation("Created role '{Role}'", roleName);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError(
                        "Failed to create role '{Role}': {Errors}", roleName, errors);

                    throw new InvalidOperationException(
                        $"Role seeding failed for '{roleName}': {errors}");
                }
            }
        }
    }
}
