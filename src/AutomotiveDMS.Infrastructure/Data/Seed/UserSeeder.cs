using AutomotiveDMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Data.Seed
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

            var email = configuration["Seed:AdminEmail"] ?? "admin@automotivedms.com";
            var password = configuration["Seed:AdminPassword"] ?? throw new InvalidOperationException(
                "Seed:AdminPassword must be configured. " +
                "Set it in appsettings.Development.json (dev) or Azure Key Vault (prod).");
            var firstName = configuration["Seed:AdminFirstName"] ?? "System";
            var lastName = configuration["Seed:AdminLastName"] ?? "Administrator";

            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser is not null)
            {
                logger.LogDebug(
                    "Admin user '{Email}' already exists - skipping", email);
                return;
            }

            var adminUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(",", createResult.Errors.Select(e => e.Description));
                logger.LogError(
                    "Failed to create admin user '{Email}': {Errors}", email, errors);

                throw new InvalidOperationException(
                    $"Admin user seeding failed: {errors}");
            }

            logger.LogInformation(
                "Created admin user '{Email}'", email);

            var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                logger.LogError(
                    "Failed to assign Admin role to '{Email}': {Errors}", email, errors);

                throw new InvalidOperationException(
                    $"Admin role assignment failed: {errors}");
            }

            logger.LogInformation(
                "Assigned 'Admin' role to '{Email}'", email);
        }
    }
}
