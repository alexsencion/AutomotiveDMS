using AutomotiveDMS.Infrastructure.Data;
using AutomotiveDMS.Infrastructure.Data.Seed;
using AutomotiveDMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.IntegrationTests.Fixtures
{
    public class WebAppFixture : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private const string EnvironmentName = "IntegrationTest";

        public string AdminEmail { get; private set; } = string.Empty;
        public string AdminPassword { get; private set; } = string.Empty;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(EnvironmentName);
        }

        public async Task InitializeAsync()
        {
            using var scope = Services.CreateScope();
            var sp = scope.ServiceProvider;
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            var config = sp.GetRequiredService<IConfiguration>();
            var context = sp.GetRequiredService<ApplicationDbContext>();

            if (!env.IsEnvironment(EnvironmentName))
                throw new InvalidOperationException(
                    $"Expected environment '{EnvironmentName}' but got '{env.EnvironmentName}'.");

            var dbName = context.Database.GetDbConnection().Database;
            if (!dbName.EndsWith("-Test", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Integration tests refused to run against database '{dbName}'. " +
                    "Expected a database whose name ends with '-Test'.");

            AdminEmail = config["Seed:AdminEmail"]
                ?? throw new InvalidOperationException("Seed:AdminEmail is not configured.");
            AdminPassword = config["Seed:AdminPassword"]
                ?? throw new InvalidOperationException("Seed:AdminPassword is not configured.");

            await context.Database.MigrateAsync();
            await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
            await EnsureAdminInKnownStateAsync(sp);
        }

        private async Task EnsureAdminInKnownStateAsync(IServiceProvider sp)
        {
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            var admin = await userManager.FindByEmailAsync(AdminEmail)
                ?? throw new InvalidOperationException($"Seeded admin '{AdminEmail}' not found.");

            if (!await userManager.CheckPasswordAsync(admin, AdminPassword))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(admin);
                var result = await userManager.ResetPasswordAsync(admin, token, AdminPassword);

                if (!result.Succeeded)
                    throw new InvalidOperationException(
                        "Failed to reset seeded admin password: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await userManager.SetLockoutEndDateAsync(admin, null);
            await userManager.ResetAccessFailedCountAsync(admin);

            if (!admin.IsActive)
            {
                admin.IsActive = true;
                await userManager.UpdateAsync(admin);
            }
        }

        public new async Task DisposeAsync()
        {
            await Task.CompletedTask;
            base.Dispose();
        }
    }
}
