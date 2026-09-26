using AutomotiveDMS.Infrastructure.Data;
using AutomotiveDMS.Infrastructure.Data.Seed;
using AutomotiveDMS.Infrastructure.Identity;
using AutomotiveDMS.IntegrationTests.Fixtures;
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
    [Collection("Integration Tests Collection")]
    public class DatabaseSeederTests : IDisposable
    {
        private readonly IServiceScope _scope;

        public DatabaseSeederTests(WebAppFixture fixture)
        {
            _scope = fixture.Services.CreateScope();
        }

        public void Dispose() => _scope.Dispose();

        [Fact]
        public async Task RoleSeeder_AllThreeRoles_Exist()
        {
            var roleManager = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roleNames = roleManager.Roles.Select(r => r.Name).ToList();

            roleNames.Should().Contain(RoleSeeder.Roles);
        }

        [Fact]
        public async Task RoleSeeder_IsIdempotent_WhenCalledTwice()
        {
            var roleManager = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var countBefore = roleManager.Roles.Count();

            await RoleSeeder.SeedAsync(_scope.ServiceProvider);

            var countAfter = roleManager.Roles.Count();
            countAfter.Should().Be(countBefore);
        }

        [Fact]
        public async Task RoleSeeder_RoleNames_MatchExpected()
        {
            var roleManager = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roleNames = roleManager.Roles.Select(r => r.Name).OrderBy(n => n).ToList();

            roleNames.Should().BeEquivalentTo(
                new[] { "Admin", "Manager", "Secretary" }.OrderBy(n => n));
        }

        [Fact]
        public async Task UserSeeder_CreatesAdminUser()
        {
            var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
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
            var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync("admin@test.automotivedms.com");
            var roles = await userManager.GetRolesAsync(user!);

            roles.Should().ContainSingle()
                .Which.Should().Be("Admin");
        }

        [Fact]
        public async Task UserSeeder_IsIdempotent_WhenCalledTwice()
        {
            var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var countBefore = userManager.Users.Count();

            await UserSeeder.SeedAsync(_scope.ServiceProvider);

            var countAfter = userManager.Users.Count();
            countAfter.Should().Be(countBefore);
        }

        [Fact]
        public async Task UserSeeder_AdminPassword_IsHashedNotPlaintext()
        {
            var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync("admin@test.automotivedms.com");

            user!.PasswordHash.Should().NotBe("TestAdmin@12345!");
            user!.PasswordHash.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ZoneSeeder_CreatesThreeDefaultZones()
        {
            var context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var zoneNames = await context.Zones.Select(z => z.Name).ToListAsync();

            zoneNames.Should().Contain(new[] { "Dealership", "Repair Shop", "In Transit" });
        }

        [Fact]
        public async Task ZoneSeeder_ZoneNames_MatchExpected()
        {
            var context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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
            var context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var allActive = await context.Zones.AllAsync(z => z.IsActive);

            allActive.Should().BeTrue();
        }

        [Fact]
        public async Task ZoneSeeder_AllZones_HaveSystemCreatedBy()
        {
            var context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var allSystemZones = await context.Zones
                .Where(z => new[] { "Dealership", "In Transit", "Repair Shop" }.Contains(z.Name))
                .AllAsync(z => z.CreatedBy == "SYSTEM");

            allSystemZones.Should().BeTrue();
        }

        [Fact]
        public async Task ZoneSeeder_IsIdempotent_WhenCalledTwice()
        {
            var context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var countBefore = await context.Zones.CountAsync();

            await ZoneSeeder.SeedAsync(_scope.ServiceProvider);

            var countAfter = await context.Zones.CountAsync();
            countAfter.Should().Be(countBefore);
        }

        [Fact]
        public async Task DatabaseSeeder_IsIdempotent_WhenCalledTwice()
        {
            var context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var roleCountBefore = roleManager.Roles.Count();
            var userCountBefore = userManager.Users.Count();
            var zoneCountBefore = await context.Zones.CountAsync();

            roleManager.Roles.Count().Should().Be(roleCountBefore);
            userManager.Users.Count().Should().Be(userCountBefore);
            (await context.Zones.CountAsync()).Should().Be(zoneCountBefore);
        }
    }
}
