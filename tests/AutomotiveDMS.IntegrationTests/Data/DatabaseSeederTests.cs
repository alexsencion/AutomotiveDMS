using AutomotiveDMS.Infrastructure.Data;
using AutomotiveDMS.Infrastructure.Data.Seed;
using AutomotiveDMS.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AutomotiveDMS.IntegrationTests.Data
{
    public class DatabaseSeederTests : IAsyncLifetime
    {
        private readonly ServiceProvider _services;
        private readonly IConfiguration _configuration;

        public DatabaseSeederTests()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.IntegrationTest.json", optional: true)
                .AddEnvironmentVariables();

            _configuration = configBuilder.Build();

            var connectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
                _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found. " +
                    "Provide via: " +
                    "1. appsettings.Integration.json (locally, gitignored) " +
                    "2. ConnectionStrings__DefaultConnection environment variable (CI)");
            }

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            serviceCollection
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            serviceCollection.AddSingleton(_configuration);

            serviceCollection.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));

            _services = serviceCollection.BuildServiceProvider();
        }

        public async Task DisposeAsync()
        {
            var context = _services.GetRequiredService<ApplicationDbContext>();
            await CleanupDatabaseAsync();

            _services.Dispose();
        }

        public async Task InitializeAsync()
        {
            var context = _services.GetRequiredService<ApplicationDbContext>();

            try
            {
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    throw new InvalidOperationException(
                        "Cannot connect to Azure SQL test database. " +
                        "Verify appsettings.IntegrationTest.json connection string and Azure credentials.");
                }

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    Console.WriteLine($"Applying {pendingMigrations.Count()} pending migrations(s) to test database...");
                    await context.Database.MigrateAsync();
                }

                await VerifySchemaAsync(context);

                await CleanupDatabaseAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Test database initialization failed. " +
                    "Ensure the Azure SQL test database is set up with migrations applied. " +
                    "Local: create appsettings.IntegrationTest.json with connection string " +
                    "CI: ensure ConnectionStrings__DefaultConnection secret is set in GitHub",
                    ex);
            }
        }

        private async Task CleanupDatabaseAsync()
        {
            var context = _services.GetRequiredService<ApplicationDbContext>();

            await context.Database.ExecuteSqlRawAsync("DELETE FROM [AuditLogs]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [CommunicationLogs]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [CustomerInteractionNotes]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [Documents]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [Payments]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [PromissoryNotes]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [PaymentSchedules]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [Guarantors]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [FinancingContracts]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [VehiclePriceHistory]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [VehicleZoneHistory]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [VehicleStatusHistory]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [Vehicles]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [Zones]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [Customers]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [AspNetUserRoles]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [AspNetUsers]");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [AspNetRoles]");

            await context.Database.ExecuteSqlRawAsync(
                "DBCC CHECKIDENT ('Zones', RESEED, 0)");
            await context.Database.ExecuteSqlRawAsync(
                "DBCC CHECKIDENT ('Vehicles', RESEED, 0)");
            await context.Database.ExecuteSqlRawAsync(
                "DBCC CHECKIDENT ('Customers', RESEED, 0)");
            await context.Database.ExecuteSqlRawAsync(
                "DBCC CHECKIDENT ('AuditLogs', RESEED, 0)");
        }

        private async Task VerifySchemaAsync(ApplicationDbContext context)
        {
            var expectedTables = new[]
            {
                "AspNetUsers", "AspNetRoles", "AspNetUserRoles",
                "Zones", "Vehicles", "Customers", "FinancingContracts",
                "Payments", "PaymentSchedules", "PromissoryNotes",
                "Documents", "AuditLogs"
            };

            var query = @"
                SELECT TABLE_NAME 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME IN ({0})";

            var parametrizedQuery = string.Format(query,
                string.Join(",", expectedTables.Select((_, i) => $"'{expectedTables[i]}'")));

            var existingTables = await context.Database.SqlQueryRaw<string>(
                "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'")
                .ToListAsync();

            foreach (var table in expectedTables)
            {
                if (!existingTables.Contains(table))
                {
                    throw new InvalidOperationException(
                        $"Required table '{table}' not found in test database. " +
                        "Run migrations before running tests.");
                }
            }

            Console.WriteLine($"✓ Schema verified: {existingTables.Count} tables present");
        }

        [Fact]
        public async Task RoleSeeder_CreatesAllFiveRoles()
        {
            await RoleSeeder.SeedAsync(_services);

            var roleManager = _services.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in RoleSeeder.Roles)
            {
                var exists = await roleManager.RoleExistsAsync(role);
                exists.Should().BeTrue(because: $"role '{role}' should have been seeded");
            }
        }

        [Fact]
        public async Task RoleSeeder_IsIdempotent_WhenCalledTwice()
        {
            await RoleSeeder.SeedAsync( _services);
            await RoleSeeder.SeedAsync(_services);

            var roleManager = _services.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = roleManager.Roles.ToList();

            roles.Should().HaveCount(RoleSeeder.Roles.Length);
        }

        [Fact]
        public async Task RoleSeeder_CreatesExactlyThreeRoles()
        {
            await RoleSeeder.SeedAsync(_services);

            var roleManager = _services.GetRequiredService<RoleManager<IdentityRole>>();
            var roleCount = roleManager.Roles.Count();

            roleCount.Should().Be(3);
        }

        [Fact]
        public async Task RoleSeeder_RoleNames_MatchExpected()
        {
            await RoleSeeder.SeedAsync(_services);

            var roleManager = _services.GetRequiredService<RoleManager<IdentityRole>>();
            var roleNames = roleManager.Roles.Select(r => r.Name).OrderBy(n => n).ToList();

            roleNames.Should().BeEquivalentTo(
                new[] { "Admin", "Manager", "Secretary" }.OrderBy(n => n));
        }

        [Fact]
        public async Task UserSeeder_CreatesAdminUser()
        {
            await RoleSeeder.SeedAsync(_services);
            await UserSeeder.SeedAsync(_services);

            var userManager = _services.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync("admin@test.automotivedms.com");

            user.Should().NotBeNull();
            user!.FirstName.Should().Be("Test");
            user.LastName.Should().Be("Administrator");
            user.IsActive.Should().BeTrue();
            user.EmailConfirmed.Should().BeTrue();
        }

        [Fact]
        public async Task UserSeeder_AssignsAdminRole()
        {
            await RoleSeeder.SeedAsync(_services);
            await UserSeeder.SeedAsync(_services);

            var userManager = _services.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync("admin@test.automotivedms.com");
            var roles = await userManager.GetRolesAsync(user!);

            roles.Should().ContainSingle()
                .Which.Should().Be("Admin");
        }

        [Fact]
        public async Task UserSeeder_IsIdempotent_WhenCalledTwice()
        {
            await RoleSeeder.SeedAsync(_services);
            await UserSeeder.SeedAsync(_services);
            await UserSeeder.SeedAsync(_services);

            var userManager = _services.GetRequiredService<UserManager<ApplicationUser>>();
            var userCount = userManager.Users.Count();

            userCount.Should().Be(1);
        }

        [Fact]
        public async Task UserSeeder_AdminPassword_IsHashedNotPlaintext()
        {
            await RoleSeeder.SeedAsync(_services);
            await UserSeeder.SeedAsync(_services);

            var userManager = _services.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync("admin@test.automotivedms.com");

            user!.PasswordHash.Should().NotBe("TestAdmin@12345!");
            user.PasswordHash.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ZoneSeeder_CreatesThreeDefaultZones()
        {
            await ZoneSeeder.SeedAsync(_services);

            var context = _services.GetRequiredService<ApplicationDbContext>();
            var zoneCount = await context.Zones.CountAsync();

            zoneCount.Should().Be(3);
        }

        [Fact]
        public async Task ZoneSeeder_ZoneNames_MatchExpected()
        {
            await ZoneSeeder.SeedAsync(_services);

            var context = _services.GetRequiredService<ApplicationDbContext>();
            var zoneNames = await context.Zones.Select(z => z.Name).OrderBy(n => n).ToListAsync();

            zoneNames.Should().BeEquivalentTo(new[]
            {
                "Dealership",
                "In Transit",
                "Repair Shop"
            }.OrderBy(n => n));
        }

        [Fact]
        public async Task ZoneSeeder_AllZones_AreActive()
        {
            await ZoneSeeder.SeedAsync(_services);

            var context = _services.GetRequiredService<ApplicationDbContext>();
            var allActive = await context.Zones.AllAsync(z => z.IsActive);

            allActive.Should().BeTrue();
        }

        [Fact]
        public async Task ZoneSeeder_AllZones_HaveSystemCreatedBy()
        {
            await ZoneSeeder.SeedAsync(_services);

            var context = _services.GetRequiredService<ApplicationDbContext>();
            var allSystemZones = await context.Zones.AllAsync(z => z.CreatedBy == "SYSTEM");

            allSystemZones.Should().BeTrue();
        }

        [Fact]
        public async Task ZoneSeeder_IsIdempotent_WhenCalledTwice()
        {
            await ZoneSeeder.SeedAsync(_services);
            await ZoneSeeder.SeedAsync(_services);

            var context = _services.GetRequiredService<ApplicationDbContext>();
            var zoneCount = await context.Zones.CountAsync();

            zoneCount.Should().Be(3);
        }

        [Fact]
        public async Task DatabaseSeeder_SeedsAllEntities_InCorrectOrder()
        {
            await DatabaseSeeder.SeedAsync(_services);

            var context = _services.GetRequiredService<ApplicationDbContext>();
            var userManager = _services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = _services.GetRequiredService<RoleManager<IdentityRole>>();

            roleManager.Roles.Count().Should().Be(3);

            var adminUser = await userManager.FindByEmailAsync("admin@test.automotivedms.com");
            adminUser.Should().NotBeNull();

            var zoneCount = await context.Zones.CountAsync();
            zoneCount.Should().Be(3);
        }

        [Fact]
        public async Task DatabaseSeeder_IsIdempotent_WhenCalledTwice()
        {
            await DatabaseSeeder.SeedAsync(_services);
            await DatabaseSeeder.SeedAsync(_services);

            var context = _services.GetRequiredService<ApplicationDbContext>();
            var userManager = _services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = _services.GetRequiredService<RoleManager<IdentityRole>>();

            roleManager.Roles.Count().Should().Be(3);
            userManager.Users.Count().Should().Be(1);
            (await context.Zones.CountAsync()).Should().Be(3);
        }
    }
}
