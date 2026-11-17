using AutomotiveDMS.Domain.Entities;
using AutomotiveDMS.Domain.Enums;
using AutomotiveDMS.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace AutomotiveDMS.IntegrationTests.Fixtures
{
    public abstract class AuditTestBase(WebAppFixture fixture) : UserManagementTestBase(fixture)
    {
        protected const string TestIp = "203.0.113.7";

        private readonly List<int> _zoneIds = [];
        private readonly List<int> _vehicleIds = [];

        public override async Task DisposeAsync()
        {
            var zoneKeys = _zoneIds.Select(i  => i.ToString(CultureInfo.InvariantCulture)).ToList();
            var vehicleKeys = _vehicleIds.Select(i => i.ToString(CultureInfo.InvariantCulture)).ToList();

            await WithDbAsync(async db =>
            {
                await db.Vehicles.Where(v => _vehicleIds.Contains(v.Id)).ExecuteDeleteAsync();
                await db.Zones.Where(v => _zoneIds.Contains(v.Id)).ExecuteDeleteAsync();
                await db.AuditLogs
                    .Where(a => (a.EntityName == "Zone" && zoneKeys.Contains(a.EntityId)) ||
                                (a.EntityName == "Vehicle" && vehicleKeys.Contains(a.EntityId)))
                    .ExecuteDeleteAsync();
            });

            await base.DisposeAsync();
        }

        protected IDisposable ActAs(string userId, string? ip = TestIp)
        {
            var accessor = Fixture.Services.GetRequiredService<IHttpContextAccessor>();

            var context = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, userId)], authenticationType: "Test"))
            };
            if (ip is not null)
                context.Connection.RemoteIpAddress = IPAddress.Parse(ip);

            accessor.HttpContext = context;
            return new ResetOnDispose(() => accessor.HttpContext = null);
        }

        private sealed class ResetOnDispose(Action reset) : IDisposable
        {
            public void Dispose() => reset();
        }

        protected async Task<string> AdminIdAsync() => (await FindUserAsync(Fixture.AdminEmail))!.Id;

        protected async Task<int> CreateZoneAsync()
        {
            var id = await WithDbAsync(async db =>
            {
                var zone = new Zone { Name = $"IT-{Guid.NewGuid():N}", Description = "Integration test zone" };
                db.Zones.Add(zone);
                await db.SaveChangesAsync();
                return zone.Id;
            });

            _zoneIds.Add(id);
            return id;
        }

        protected Task UpdateZoneAsync(int id, Action<Zone> change) => WithDbAsync(async db =>
        {
            change(await db.Zones.SingleAsync(z => z.Id == id));
            await db.SaveChangesAsync();
        });

        protected async Task<int> CreateVehicleAsync(int zoneId, decimal listPrice = 18500m)
        {
            var id = await WithDbAsync(async db =>
            {
                var vehicle = new Vehicle
                {
                    VIN = ("IT" + Guid.NewGuid().ToString("N").ToUpperInvariant())[..17],
                    Make = "Toyota",
                    Model = "Corolla",
                    Year = 2024,
                    PurchasePrice = 15000m,
                    ListPrice = listPrice,
                    Status = VehicleStatus.Available,
                    ZoneId = zoneId
                };
                db.Vehicles.Add(vehicle);
                await db.SaveChangesAsync();
                return vehicle.Id;
            });

            _vehicleIds.Add(id);
            return id;
        }

        protected Task UpdateVehicleAsync(int id, Action<Vehicle> change) => WithDbAsync(async db =>
        {
            change(await db.Vehicles.SingleAsync(v => v.Id == id));
            await db.SaveChangesAsync();
        });

        protected Task<List<AuditLog>> AuditRowsAsync(string entityName, object entityId)
        {
            var key = Convert.ToString(entityId, CultureInfo.InvariantCulture)!;
            return WithDbAsync(db => db.AuditLogs.AsNoTracking() 
                .Where(a => a.EntityName == entityName && a.EntityId == key)
                .OrderBy(a => a.Id)
                .ToListAsync());
        }

        protected async Task WithDbAsync(Func<ApplicationDbContext, Task> action)
        {
            using var scope = Fixture.Services.CreateScope();
            await action(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
        }

        protected async Task<T> WithDbAsync<T>(Func<ApplicationDbContext, Task<T>> action)
        {
            using var scope = Fixture.Services.CreateScope();
            return await action(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
        }

        protected async Task<T> WithServiceAsync<TService, T>(Func<TService, Task<T>> action)
            where TService : notnull
        {
            using var scope = Fixture.Services.CreateScope();
            return await action(scope.ServiceProvider.GetRequiredService<TService>());
        }
    }
}
