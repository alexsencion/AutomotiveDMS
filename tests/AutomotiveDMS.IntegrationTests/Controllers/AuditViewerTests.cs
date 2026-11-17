using AutomotiveDMS.Application.Common.Models;
using AutomotiveDMS.Application.DTOs.Audit;
using AutomotiveDMS.Application.Interfaces.Services;
using AutomotiveDMS.Domain.Enums;
using AutomotiveDMS.Infrastructure.Identity;
using AutomotiveDMS.IntegrationTests.Fixtures;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AutomotiveDMS.IntegrationTests.Controllers
{
    [Collection(IntegrationTestCollection.Name)]
    public class AuditViewerTests(WebAppFixture fixture)
        : AuditTestBase(fixture), IClassFixture<WebAppFixture>
    {
        [Fact]
        public async Task Page_Anonymous_RedirectsToLogin()
        {
            using var client = CreateClient();

            var response = await client.GetAsync("/Audit");

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("/Account/Login");
        }

        [Fact]
        public async Task Page_Secretary_IsDenied()
        {
            var (_, email) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var client = await LoginAsync(email, KnownPassword);

            var response = await client.GetAsync("/Audit");

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("/Account/AccessDenied");
        }

        [Fact]
        public async Task Page_NoFilters_ListsEntries()
        {
            var zoneId = await CreateZoneAsync();
            using var admin = await LoginAsAdminAsync();

            var html = await (await admin.GetAsync("/Audit")).Content.ReadAsStringAsync();

            html.Should().Contain($"Zone #{zoneId}").And.NotContain("No audit entries match");
        }

        [Fact]
        public async Task Page_ActionAndRecordFilters_BindFromQueryString()
        {
            var zoneId = await CreateZoneAsync();
            await UpdateZoneAsync(zoneId, z => z.IsActive = false);
            using var admin = await LoginAsAdminAsync();

            var deleted = await (await admin.GetAsync(
                $"/Audit?EntityName=Zone&EntityId={zoneId}&ActionFilter=Deleted")).Content.ReadAsStringAsync();
            deleted.Should().Contain("IsActive").And.NotContain("No audit entries match");

            var restored = await (await admin.GetAsync(
                $"/Audit?EntityName=Zone&EntityId={zoneId}&ActionFilter=Restored")).Content.ReadAsStringAsync();
            restored.Should().Contain("No audit entries match");
        }

        [Fact]
        public async Task Page_FromAfterTo_ShowsValidationMessage()
        {
            using var admin = await LoginAsAdminAsync();

            var response = await admin.GetAsync("/Audit?From=2026-11-10&To=2026-11-01");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().ContainEquivalentOf("Must be on or after");
        }

        [Fact]
        public async Task Page_MalformedDate_ShowsBindingError_InsteadOfSilentlyEmptyList()
        {
            using var admin = await LoginAsAdminAsync();

            var response = await admin.GetAsync("/Audit?From=not-a-date");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Contain("is not valid");
        }

        [Fact]
        public async Task Search_ByRecord_ReturnsNewestFirst()
        {
            var zoneId = await CreateZoneAsync();
            await UpdateZoneAsync(zoneId, z => z.Description = "first");
            await UpdateZoneAsync(zoneId, z => z.Description = "second");

            var result = await SearchAsync(new AuditLogQuery(EntityName: "Zone", EntityId: zoneId.ToString()));

            result.Items.Select(i => i.Id).Should().BeInDescendingOrder();
            result.Items.Select(i => i.Action).Should().Equal(
                AuditAction.Updated, AuditAction.Updated, AuditAction.Created);
            result.Items[0].NewValue.Should().Be("second");
        }

        [Fact]
        public async Task Search_ByChangedBy_SeparatesUsersFromSystem()
        {
            var zoneId = await CreateZoneAsync();
            var adminId = await AdminIdAsync();
            using (ActAs(adminId))
            {
                await UpdateZoneAsync(zoneId, z => z.Description = "by admin");
            }

            var byAdmin = await SearchAsync(new AuditLogQuery(EntityId: zoneId.ToString(), ChangedBy: adminId));
            var bySystem = await SearchAsync(new AuditLogQuery(EntityId: zoneId.ToString(), ChangedBy: "SYSTEM"));

            byAdmin.Items.Should().ContainSingle().Which.Action.Should().Be(AuditAction.Updated);
            bySystem.Items.Should().ContainSingle().Which.Action.Should().Be(AuditAction.Created);
        }

        [Fact]
        public async Task Search_ByDateRange_IncludesOnlyEntriesInsideTheRange()
        {
            var zoneId = await CreateZoneAsync();
            var key = zoneId.ToString();
            var now = DateTime.UtcNow;

            (await SearchAsync(new AuditLogQuery(EntityId: key, FromUtc: now.AddHours(-1), ToUtcExclusive: now.AddHours(1))))
                .TotalCount.Should().Be(1);
            (await SearchAsync(new AuditLogQuery(EntityId: key, FromUtc: now.AddHours(1))))
                .TotalCount.Should().Be(0);
            (await SearchAsync(new AuditLogQuery(EntityId: key, ToUtcExclusive: now.AddHours(-1))))
                .TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Search_Paging_ReportsPagesAndNavigation()
        {
            var zoneId = await CreateZoneAsync();
            for (var i = 1; i <= 3; i++)
                await UpdateZoneAsync(zoneId, z => z.Description = $"change {i}");

            var key = zoneId.ToString();
            var page1 = await SearchAsync(new AuditLogQuery(EntityName: "Zone", EntityId: key, Page: 1, PageSize: 2));
            var page2 = await SearchAsync(new AuditLogQuery(EntityName: "Zone", EntityId: key, Page: 2, PageSize: 2));

            page1.Items.Should().HaveCount(2);
            page1.TotalCount.Should().Be(4);
            page1.TotalPages.Should().Be(2);
            page1.HasPrevious.Should().BeFalse();
            page1.HasNext.Should().BeTrue();

            page2.Items.Should().HaveCount(2);
            page2.HasPrevious.Should().BeTrue();
            page2.HasNext.Should().BeFalse();
            page1.Items.Select(i => i.Id).Should().NotIntersectWith(page2.Items.Select(i => i.Id));
        }

        [Fact]
        public async Task Search_ClampsInvalidPageAndPageSize()
        {
            var result = await SearchAsync(new AuditLogQuery(Page: 0, PageSize: 1000));

            result.Page.Should().Be(1);
            result.PageSize.Should().Be(100);
        }

        [Fact]
        public async Task Search_ResolvesActorAndRecordDisplayNames()
        {
            var admin = (await FindUserAsync(Fixture.AdminEmail))!;
            var zoneId = await CreateZoneAsync();
            using (ActAs(admin.Id))
            {
                await UpdateZoneAsync(zoneId, z => z.Description = "named");
            }
            var (userId, _) = await CreateUserDirectlyAsync(AppRoles.Secretary);

            var zoneRows = (await SearchAsync(new AuditLogQuery(EntityName: "Zone", EntityId: zoneId.ToString()))).Items;
            var userRows = (await SearchAsync(new AuditLogQuery(EntityName: "User", EntityId: userId))).Items;

            zoneRows.Should().AllSatisfy(r => r.EntityDisplay.Should().Be($"Zone #{zoneId}"));
            zoneRows.Single(r => r.Action == AuditAction.Updated).ChangedByName.Should().Be(admin.FullName);
            zoneRows.Single(r => r.Action == AuditAction.Created).ChangedByName.Should().Be("SYSTEM");
            userRows.Should().NotBeEmpty().And.AllSatisfy(r => r.EntityDisplay.Should().Be("Direct User"));
        }

        [Fact]
        public async Task EntityNames_AreDistinctAndSorted()
        {
            await CreateZoneAsync();
            await CreateUserDirectlyAsync(AppRoles.Secretary);

            var names = await WithServiceAsync<IAuditService, IReadOnlyList<string>>(s => s.GetEntityNamesAsync());

            names.Should().Contain(["User", "Zone"])
                .And.OnlyHaveUniqueItems()
                .And.BeInAscendingOrder();
        }

        private Task<PagedResult<AuditLogEntryDto>> SearchAsync(AuditLogQuery query) =>
            WithServiceAsync<IAuditService, PagedResult<AuditLogEntryDto>>(s => s.SearchAsync(query));
    }
}
