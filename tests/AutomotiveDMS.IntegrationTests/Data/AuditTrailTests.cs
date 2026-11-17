using AutomotiveDMS.Domain.Entities;
using AutomotiveDMS.Domain.Enums;
using AutomotiveDMS.Infrastructure.Identity;
using AutomotiveDMS.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AutomotiveDMS.IntegrationTests.Data
{
    [Collection(IntegrationTestCollection.Name)]
    public class AuditTrailTests(WebAppFixture fixture)
        : AuditTestBase(fixture), IClassFixture<WebAppFixture>
    {
        [Fact]
        public async Task Created_IntKeyedEntity_RecordsRealGeneratedId()
        {
            var zoneId = await CreateZoneAsync();

            zoneId.Should().BePositive();
            var row = (await AuditRowsAsync("Zone", zoneId)).Should().ContainSingle().Subject;
            row.Action.Should().Be(AuditAction.Created);
            row.FieldName.Should().BeNull();
        }

        [Fact]
        public async Task Created_WithSignedInUser_StampsCreatedBy_AndRecordsActorAndIp()
        {
            var adminId = await AdminIdAsync();
            using var _ = ActAs(adminId);

            var zoneId = await CreateZoneAsync();

            var zone = await WithDbAsync(db => db.Zones.AsNoTracking().SingleAsync(z => z.Id == zoneId));
            zone.CreatedBy.Should().Be(adminId);
            zone.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

            var row = (await AuditRowsAsync("Zone", zoneId)).Single();
            row.ChangedBy.Should().Be(adminId);
            row.IpAddress.Should().Be(TestIp);
        }

        [Fact]
        public async Task Created_OutsideAnyRequest_IsAttributedToSystem()
        {
            var zoneId = await CreateZoneAsync();

            (await WithDbAsync(db => db.Zones.AsNoTracking().SingleAsync(z => z.Id == zoneId)))
                .CreatedBy.Should().Be("SYSTEM");

            var row = (await AuditRowsAsync("Zone", zoneId)).Single();
            row.ChangedBy.Should().Be("SYSTEM");
            row.IpAddress.Should().BeNull();
        }

        [Fact]
        public async Task Update_WritesOneRowPerChangedField_AndStampsModified()
        {
            var adminId = await AdminIdAsync();
            using var _ = ActAs(adminId);
            var zoneId = await CreateZoneAsync();

            await UpdateZoneAsync(zoneId, z =>
            {
                z.Name = $"IT-{Guid.NewGuid():N}";
                z.Description = "Renamed";
            });

            var updates = (await AuditRowsAsync("Zone", zoneId))
                .Where(r => r.Action == AuditAction.Updated).ToList();

            updates.Select(r => r.FieldName).Should().BeEquivalentTo(["Name", "Description"]);
            updates.Should().AllSatisfy(r =>
            {
                r.ChangedBy.Should().Be(adminId);
                r.IpAddress.Should().Be(TestIp);
            });
            updates.Single(r => r.FieldName == "Description")
                .Should().Match<AuditLog>(r => r.OldValue == "Integration test zone" && r.NewValue == "Renamed");

            updates.Should().NotContain(r => r.FieldName == "ModifiedBy" || r.FieldName == "ModifiedDate");
            var zone = await WithDbAsync(db => db.Zones.AsNoTracking().SingleAsync(z => z.Id == zoneId));
            zone.ModifiedBy.Should().Be(adminId);
            zone.ModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task Update_CannotOverwriteCreatedByOrCreatedDate()
        {
            var zoneId = await CreateZoneAsync();
            var adminId = await AdminIdAsync();
            using var _ = ActAs(adminId);

            await WithDbAsync(async db =>
            {
                var zone = await db.Zones.SingleAsync(z => z.Id == zoneId);
                zone.CreatedBy = "tampered";
                zone.CreatedDate = DateTime.UtcNow.AddYears(-5);
                db.Zones.Update(zone);
                await db.SaveChangesAsync();
            });

            var stored = await WithDbAsync(db => db.Zones.AsNoTracking().SingleAsync(z => z.Id == zoneId));
            stored.CreatedBy.Should().Be("SYSTEM");
            stored.CreatedDate.Should().BeAfter(DateTime.UtcNow.AddYears(-1));
            stored.ModifiedBy.Should().Be(adminId);
        }

        [Fact]
        public async Task Update_ReassigningSameValues_WritesNoRows()
        {
            var zoneId = await CreateZoneAsync();

            await WithDbAsync(async db =>
            {
                db.Zones.Update(await db.Zones.SingleAsync(z => z.Id == zoneId));
                await db.SaveChangesAsync();
            });

            (await AuditRowsAsync("Zone", zoneId)).Should().ContainSingle()
                .Which.Action.Should().Be(AuditAction.Created);
        }

        [Fact]
        public async Task Update_DecimalsAndEnums_RoundTripThroughDatabase_AndAreRecordedReadably()
        {
            var zoneId = await CreateZoneAsync();
            var vehicleId = await CreateVehicleAsync(zoneId, listPrice: 18500m);

            await UpdateVehicleAsync(vehicleId, v => v.ListPrice = 18500m);
            (await AuditRowsAsync("Vehicle", vehicleId)).Should().ContainSingle(
                "equal decimals with a different scale are not a change");

            await UpdateVehicleAsync(vehicleId, v =>
            {
                v.ListPrice = 17999.5m;
                v.Status = VehicleStatus.Sold;
            });

            var updates = (await AuditRowsAsync("Vehicle", vehicleId))
                .Where(r => r.Action == AuditAction.Updated).ToList();

            updates.Should().ContainEquivalentOf(new { FieldName = "ListPrice", OldValue = "18500.00", NewValue = "17999.50" });
            updates.Should().ContainEquivalentOf(new { FieldName = "Status", OldValue = "Available", NewValue = "Sold" });
        }

        [Fact]
        public async Task IsActiveTransactions_AreRecordedAsDeletedThenRestored()
        {
            var zoneId = await CreateZoneAsync();

            await UpdateZoneAsync(zoneId, z => z.IsActive = false);
            await UpdateZoneAsync(zoneId, z => z.IsActive = true);

            (await AuditRowsAsync("Zone", zoneId)).Select(r => r.Action).Should().Equal(
                AuditAction.Created, AuditAction.Deleted, AuditAction.Restored);
        }

        [Fact]
        public async Task HardDelete_WritesDeletedRow_WithoutFieldDetails()
        {
            var zoneId = await CreateZoneAsync();

            await WithDbAsync(async db =>
            {
                db.Zones.Remove(await db.Zones.SingleAsync(z => z.Id == zoneId));
                await db.SaveChangesAsync();
            });

            var last = (await AuditRowsAsync("Zone", zoneId)).Last();
            last.Action.Should().Be(AuditAction.Deleted);
            last.FieldName.Should().BeNull();
        }

        [Fact]
        public async Task FailedSave_WritesNoAuditRows()
        {
            var zoneA = await CreateZoneAsync();
            var zoneB = await CreateZoneAsync();
            var nameA = await WithDbAsync(db => db.Zones.Where(z => z.Id == zoneA).Select(z => z.Name).SingleAsync());

            var act = () => UpdateZoneAsync(zoneB, z => z.Name =  nameA);

            await act.Should().ThrowAsync<DbUpdateException>();
            (await AuditRowsAsync("Zone", zoneB)).Should().ContainSingle()
                .Which.Action.Should().Be(AuditAction.Created, "the Updated row must roll back with the change");
        }

        [Fact]
        public async Task AuditLog_CannotBeModifiedOrDeletedThroughEf()
        {
            var zoneId = await CreateZoneAsync();
            var rowId = (await AuditRowsAsync("Zone", zoneId)).Single().Id;

            var modify = () => WithDbAsync(async db =>
            {
                (await db.AuditLogs.SingleAsync(a => a.Id == rowId)).NewValue = "tampered";
                await db.SaveChangesAsync();
            });
            var delete = () => WithDbAsync(async db =>
            {
                db.AuditLogs.Remove(await db.AuditLogs.SingleAsync(a => a.Id == rowId));
                await db.SaveChangesAsync();
            });

            await modify.Should().ThrowAsync<InvalidOperationException>().WithMessage("*immutable*");
            await delete.Should().ThrowAsync<InvalidOperationException>().WithMessage("*immutable*");

            (await AuditRowsAsync("Zone", zoneId)).Should().ContainSingle()
                .Which.NewValue.Should().BeNull();
        }

        [Fact]
        public async Task UserCreatedAndRoleChanges_AreAuditedWithRoleNames()
        {
            var adminId = await AdminIdAsync();
            using var _ = ActAs(adminId);
            var email = NewEmail();

            var userId = await WithUserManagerAsync(async um =>
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = "Audit",
                    LastName = "Target",
                    IsActive = true,
                    EmailConfirmed = true
                };
                (await um.CreateAsync(user)).Succeeded.Should().BeTrue();
                (await um.AddToRoleAsync(user, AppRoles.Secretary)).Succeeded.Should().BeTrue();
                (await um.RemoveFromRoleAsync(user, AppRoles.Secretary)).Succeeded.Should().BeTrue();
                return user.Id;
            });

            var rows = await AuditRowsAsync("User", userId);

            rows.Select(r => (r.Action, r.FieldName, r.OldValue, r.NewValue)).Should().Equal(
                new (AuditAction, string?, string?, string?)[]
                {
                    (AuditAction.Created, null, null, null),
                    (AuditAction.Updated, "Role", null, "Secretary"),
                    (AuditAction.Updated, "Role", "Secretary", null),
                });
            rows.Should().AllSatisfy(r => r.ChangedBy.Should().Be(adminId));
        }

        [Fact]
        public async Task PasswordLoginAndLockoutChanges_WriteNoRows()
        {
            var (id, _) = await CreateUserDirectlyAsync(AppRoles.Manager, pending: true);
            var before = (await AuditRowsAsync("User", id)).Count;

            await WithUserManagerAsync(async um =>
            {
                var user = (await um.FindByIdAsync(id))!;
                (await um.AddPasswordAsync(user, NewPassword)).Succeeded.Should().BeTrue();
                user.LastLoginDate = DateTime.UtcNow;
                await um.UpdateAsync(user);
                await um.AccessFailedAsync(user);
                await um.ResetAccessFailedCountAsync(user);
                await um.UpdateSecurityStampAsync(user);
            });

            (await AuditRowsAsync("User", id)).Should().HaveCount(before);
        }

        [Fact]
        public async Task DeactivationThroughUi_IsAttributedToTheAdmin()
        {
            var (id, _) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            var adminId = await AdminIdAsync();
            using var admin = await LoginAsAdminAsync();

            var token = await GetAntiForgeryTokenAsync(admin, "/Users");
            var response = await admin.PostAsync($"/Users/Deactivate/{id}",
                Form(("__RequestVerificationToken", token)));

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            (await AuditRowsAsync("User", id)).Last().Should().Match<AuditLog>(r =>
                r.Action == AuditAction.Deleted && r.FieldName == "IsActive" && r.ChangedBy == adminId);
        }
    }
}
