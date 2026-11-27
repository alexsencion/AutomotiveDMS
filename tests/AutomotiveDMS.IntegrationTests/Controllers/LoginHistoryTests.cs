using AutomotiveDMS.Application.Common.Models;
using AutomotiveDMS.Application.DTOs.LoginHistory;
using AutomotiveDMS.Application.Interfaces.Services;
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

namespace AutomotiveDMS.IntegrationTests.Controllers
{
    [Collection(IntegrationTestCollection.Name)]
    public class LoginHistoryTests(WebAppFixture fixture)
        : AuditTestBase(fixture), IClassFixture<WebAppFixture>
    {
        private const string TestAgent = "AutomotiveDMS-IntegrationTests/1.0";

        public override async Task DisposeAsync()
        {
            var emails = CreatedEmails.ToArray();

            await WithDbAsync(db => db.LoginEvents.Where(e => emails.Contains(e.Email)).ExecuteDeleteAsync());
            await base.DisposeAsync();
        }

        [Fact]
        public async Task SuccessfulLogin_RecordsSucceeded_WithUserAndDevice()
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var client = CreateClientWithAgent();

            var response = await AttemptLoginAsync(client, email, KnownPassword);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            var e = (await EventsForAsync(email)).Should().ContainSingle().Subject;
            e.EventType.Should().Be(LoginEventType.Succeeded);
            e.UserId.Should().Be(id);
            e.UserAgent.Should().Be(TestAgent);
            e.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task WrongPassword_RecordsFailedPassword_ForThatUser()
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var client = CreateClient();

            await AttemptLoginAsync(client, email, GeneratePassword());

            var e = (await EventsForAsync(email)).Should().ContainSingle().Subject;
            e.EventType.Should().Be(LoginEventType.FailedPassword);
            e.UserId.Should().Be(id);
        }

        [Fact]
        public async Task UnknownEmail_RecordsFailedUnknownUser_WithoutUserId()
        {
            var email = NewEmail();
            using var client = CreateClient();

            await AttemptLoginAsync(client, email, GeneratePassword());

            var e = (await EventsForAsync(email)).Should().ContainSingle().Subject;
            e.EventType.Should().Be(LoginEventType.FailedUnknownUser);
            e.UserId.Should().BeNull();
        }

        [Fact]
        public async Task FiveFailures_TriggerLockout_ThenAttemptWhileLockedIsRecorded()
        {
            var (_, email) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var client = CreateClient();

            for (var i = 0; i < 5; i++)
                await AttemptLoginAsync(client, email, GeneratePassword());

            var locked = await AttemptLoginAsync(client, email, KnownPassword);
            (await locked.Content.ReadAsStringAsync()).Should().ContainEquivalentOf("locked");

            (await EventsForAsync(email)).Select(e => e.EventType).Should().Equal(
                LoginEventType.FailedPassword,
                LoginEventType.FailedPassword,
                LoginEventType.FailedPassword,
                LoginEventType.FailedPassword,
                LoginEventType.LockoutTriggered,
                LoginEventType.LockoutOutAttempt);
        }

        [Fact]
        public async Task DeactivatedUser_RecordsBlockedInactive()
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Manager, isActive: false);
            using var client = CreateClient();

            await AttemptLoginAsync(client, email, KnownPassword);

            var e = (await EventsForAsync(email)).Should().ContainSingle().Subject;
            e.EventType.Should().Be(LoginEventType.BlockedInactive);
            e.UserId.Should().Be(id);
        }

        [Fact]
        public async Task Logout_RecordsLoggedOut_AfterSucceeded()
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var client = await LoginAsync(email, KnownPassword);
            var token = await GetAntiForgeryTokenAsync(client, "/Dashboard");

            var response = await client.PostAsync("/Account/Logout", Form(("__RequestVerificationToken", token)));

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            var events = await EventsForAsync(email);
            events.Select(e => e.EventType).Should().Equal(LoginEventType.Succeeded, LoginEventType.LoggedOut);
            events.Last().UserId.Should().Be(id);
        }

        [Fact]
        public async Task InvalidForm_IsNotRecorded()
        {
            var malformed = $"not-an-email-{Guid.NewGuid():N}";
            using var client = CreateClient();

            var response = await AttemptLoginAsync(client, malformed, GeneratePassword());

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await EventsForAsync(malformed)).Should().BeEmpty();
        }

        [Fact]
        public async Task LoginActivity_ProducesNoAuditRows()
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            var auditBefore = await UserAuditCountAsync(id);

            using (var failing = CreateClient())
                await AttemptLoginAsync(failing, email, GeneratePassword());
            using (await LoginAsync(email, KnownPassword)) { }

            (await UserAuditCountAsync(id)).Should().Be(auditBefore, 
                "LastLoginDate, AccessFailedCount and stamps are excluded from the audit trail");
            (await WithDbAsync(db => db.AuditLogs.CountAsync(a => a.EntityName == nameof(LoginEvent))))
                .Should().Be(0, "LoginEvent is in the interceptor's NotAudited set");

        }

        [Fact]
        public async Task Record_CapturesIpAndUserAgent_FromCurrentRequest()
        {
            var email = NewEmail();

            using (ActAs("any-user", ip: TestIp, userAgent: TestAgent))
            {
                await WithServiceAsync<ILoginHistoryService>(s =>
                    s.RecordAsync(LoginEventType.FailedUnknownUser, email, userId: null));
            }

            var e = (await EventsForAsync(email)).Should().ContainSingle().Subject;
            e.IpAddress.Should().Be(TestIp);
            e.UserAgent.Should().Be(TestAgent);
        }

        [Fact]
        public async Task Record_TruncatesOversizedEmailAndUserAgent()
        {
            var marker = Guid.NewGuid().ToString("N");
            var email = marker + new string('x', 300);
            var longUa = new string('u', 600);

            try
            {
                using (ActAs("any-user", userAgent: longUa))
                {
                    await WithServiceAsync<ILoginHistoryService>(s =>
                        s.RecordAsync(LoginEventType.FailedUnknownUser, email, userId: null));
                }

                var e = await WithDbAsync(db => db.LoginEvents.AsNoTracking()
                    .SingleAsync(x => x.Email.StartsWith(marker)));
                e.Email.Should().HaveLength(256);
                e.UserAgent.Should().HaveLength(512);
            }
            finally
            {
                await WithDbAsync(db => db.LoginEvents.Where(x => x.Email.StartsWith(marker)).ExecuteDeleteAsync());
            }
        }

        [Fact]
        public async Task LoginEvent_CannotBeModifiedOrDeletedThroughEf()
        {
            var email = NewEmail();
            await WithServiceAsync<ILoginHistoryService>(s =>
                s.RecordAsync(LoginEventType.FailedUnknownUser, email, userId: null));
            var id = (await EventsForAsync(email)).Single().Id;

            var modify = () => WithDbAsync(async db =>
            {
                (await db.LoginEvents.SingleAsync(e => e.Id == id)).EventType = LoginEventType.Succeeded;
                await db.SaveChangesAsync();
            });
            var delete = () => WithDbAsync(async db =>
            {
                db.LoginEvents.Remove(await db.LoginEvents.SingleAsync(e => e.Id == id));
                await db.SaveChangesAsync();
            });

            await modify.Should().ThrowAsync<InvalidOperationException>().WithMessage("*immutable*");
            await delete.Should().ThrowAsync<InvalidOperationException>().WithMessage("*immutable*");
            (await EventsForAsync(email)).Single().EventType.Should().Be(LoginEventType.FailedUnknownUser);
        }

        [Fact]
        public async Task Page_Anonymous_RedirectsToLogin()
        {
            using var client = CreateClient();

            var response = await client.GetAsync("/LoginHistory");

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("/Account/Login");
        }

        [Fact]
        public async Task Page_Secretary_IsDenied()
        {
            var (_, email) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var client = await LoginAsync(email, KnownPassword);

            var response = await client.GetAsync($"/LoginHistory");

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("/Account/AccessDenied");
        }

        [Fact]
        public async Task Page_UserFilter_ShowsOnlyThatUsersEvents()
        {
            var (idA, emailA) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            var (_, emailB) = await CreateUserDirectlyAsync(AppRoles.Manager);
            using (await LoginAsync(emailA, KnownPassword)) { }
            using (await LoginAsync(emailB, KnownPassword)) { }
            using var admin = await LoginAsAdminAsync();

            var html = await (await admin.GetAsync($"/LoginHistory?UserId={idA}")).Content.ReadAsStringAsync();

            html.Should().Contain(emailA).And.NotContain(emailB);
        }

        [Fact]
        public async Task Page_EmailAndEventFilters_BindFromQueryString()
        {
            var email = NewEmail();
            using (var client = CreateClient())
                await AttemptLoginAsync(client, email, GeneratePassword());
            using var admin = await LoginAsAdminAsync();

            var matching = await (await admin.GetAsync(
                $"/LoginHistory?Email={email}&EventTypeFilter=FailedUnknownUser")).Content.ReadAsStringAsync();
            matching.Should().Contain(email).And.NotContainEquivalentOf("No login events match");

            var other = await (await admin.GetAsync(
                $"/LoginHistory?Email={email}&EventTypeFilter=Succeeded")).Content.ReadAsStringAsync();
            other.Should().ContainEquivalentOf("No login events match");
        }

        [Fact]
        public async Task Page_InvalidDates_ShowValidationErrors()
        {
            using var admin = await LoginAsAdminAsync();

            var reversed = await (await admin.GetAsync("/LoginHistory?From=2026-11-10&To=2026-11-01")).Content.ReadAsStringAsync();
            var malformed = await (await admin.GetAsync("LoginHistory?From=not-a-date")).Content.ReadAsStringAsync();

            reversed.Should().ContainEquivalentOf("must be on or after");
            malformed.Should().ContainEquivalentOf("is not valid");
        }

        [Fact]
        public async Task UsersPage_HasHistoryLinkPerUser()
        {
            var (id, _) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var admin = await LoginAsAdminAsync();

            var html = await (await admin.GetAsync("/Users")).Content.ReadAsStringAsync();

            html.Should().Contain($"/LoginHistory?UserId={id}");
        }

        [Fact]
        public async Task Search_PartialEmail_NewestFirst_AndDateRange()
        {
            var email = NewEmail();
            foreach (var type in new[] { LoginEventType.FailedUnknownUser, LoginEventType.FailedUnknownUser, LoginEventType.FailedUnknownUser })
                await WithServiceAsync<ILoginHistoryService>(s => s.RecordAsync(type, email, userId: null));

            var fragment = email[3..20];
            var now = DateTime.UtcNow;

            var all = await SearchAsync(new LoginHistoryQuery(Email: fragment));
            all.TotalCount.Should().Be(3);
            all.Items.Select(i => i.Id).Should().BeInDescendingOrder();

            (await SearchAsync(new LoginHistoryQuery(Email: fragment, FromUtc: now.AddHours(-1), ToUtcExclusive: now.AddHours(1))))
                .TotalCount.Should().Be(3);
            (await SearchAsync(new LoginHistoryQuery(Email: fragment, FromUtc: now.AddHours(1))))
                .TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Search_Paging_ReportsPagesAndNavigation()
        {
            var email = NewEmail();
            for (int i = 0; i < 3; i++)
                await WithServiceAsync<ILoginHistoryService>(s =>
                    s.RecordAsync(LoginEventType.FailedUnknownUser, email, userId: null));

            var page1 = await SearchAsync(new LoginHistoryQuery(Email: email, Page: 1, PageSize: 2));
            var page2 = await SearchAsync(new LoginHistoryQuery(Email: email, Page: 2, PageSize: 2));

            page1.Items.Should().HaveCount(2);
            page1.TotalPages.Should().Be(2);
            page1.HasNext.Should().BeTrue();
            page2.Items.Should().ContainSingle();
            page2.HasPrevious.Should().BeTrue();
            page2.HasNext.Should().BeFalse();
        }

        [Fact]
        public async Task Search_ResolvesUserNames_AndLeavesUnknownAccountsNull()
        {
            var (id, knownEmail) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            var unknownEmail = NewEmail();

            await WithServiceAsync<ILoginHistoryService>(s => s.RecordAsync(LoginEventType.Succeeded, knownEmail, id));
            await WithServiceAsync<ILoginHistoryService>(s => s.RecordAsync(LoginEventType.FailedUnknownUser, unknownEmail, null));

            (await SearchAsync(new LoginHistoryQuery(Email: knownEmail))).Items.Single().UserName.Should().Be("Direct User");
            (await SearchAsync(new LoginHistoryQuery(Email: unknownEmail))).Items.Single().UserName.Should().BeNull();
        }


        private HttpClient CreateClientWithAgent()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(TestAgent);
            return client;
        }

        private static async Task<HttpResponseMessage> AttemptLoginAsync(HttpClient client, string email, string password)
        {
            var token = await GetAntiForgeryTokenAsync(client, "/Account/Login");
            return await client.PostAsync("/Account/Login", Form(
                ("Email", email), ("Password", password), ("RememberMe", "false"),
                ("__RequestVerificationToken", token)));
        }

        private Task<List<LoginEvent>> EventsForAsync(string email) =>
            WithDbAsync(db => db.LoginEvents.AsNoTracking()
                .Where(e => e.Email == email)
                .OrderBy(e => e.Id)
                .ToListAsync());

        private Task<int> UserAuditCountAsync(string userId) =>
            WithDbAsync(db => db.AuditLogs.CountAsync(a => a.EntityName == "User" && a.EntityId == userId));

        private Task<PagedResult<LoginEventDto>> SearchAsync(LoginHistoryQuery query) =>
            WithServiceAsync<ILoginHistoryService, PagedResult<LoginEventDto>>(s => s.SearchAsync(query));
    }
}
