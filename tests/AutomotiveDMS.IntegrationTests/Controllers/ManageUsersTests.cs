using AutomotiveDMS.Infrastructure.Identity;
using AutomotiveDMS.IntegrationTests.Fixtures;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AutomotiveDMS.IntegrationTests.Controllers
{
    [Collection(IntegrationTestCollection.Name)]
    public class ManageUsersTests(WebAppFixture fixture)
        : UserManagementTestBase(fixture), IClassFixture<WebAppFixture>
    {
        [Fact]
        public async Task Edit_UpdatesNameAndPhone_TrimmedAndEmptyPhoneStoredAsNull()
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var admin = await LoginAsAdminAsync();

            var first = await PostEditAsync(admin, id, " Ana ", " Gómez ", AppRoles.Secretary, " 809-555-0123");
            AssertRedirectsTo(first, "/Users");

            var user = await FindUserAsync(email);
            user!.FirstName.Should().Be("Ana");
            user!.LastName.Should().Be("Gómez");
            user!.PhoneNumber.Should().Be("809-555-0123");

            await PostEditAsync(admin, id, "Ana", "Gómez", AppRoles.Secretary, phone: "");
            (await FindUserAsync(email))!.PhoneNumber.Should().BeNull();
        }

        [Theory]
        [InlineData(AppRoles.Admin)]
        [InlineData("SuperUser")]
        public async Task Edit_WithNonAssignableRole_IsRejected_AndNothingChanges(string role)
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var admin = await LoginAsAdminAsync();

            var response = await PostEditAsync(admin, id, "Changed", "Name", role);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Contain("Select a valid role.");
            (await FindUserAsync(email))!.FirstName.Should().Be("Direct");
            (await GetRolesAsync(email)).Should().ContainSingle().Which.Should().Be(AppRoles.Secretary);
        }

        [Fact]
        public async Task Edit_WithMissingNames_ShowErrors_AndNothingChanges()
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var admin = await LoginAsAdminAsync();

            var response = await PostEditAsync(admin, id, "", "", AppRoles.Manager);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync())
                .Should().Contain("First name is required").And.Contain("Last name is required");
            (await FindUserAsync(email))!.FirstName.Should().Be("Direct");
            (await GetRolesAsync(email)).Should().ContainSingle().Which.Should().Be(AppRoles.Secretary);
        }

        [Fact]
        public async Task Edit_OwnAccount_PostedRoleIsIgnored()
        {
            var self = (await FindUserAsync(Fixture.AdminEmail))!;
            using var admin = await LoginAsAdminAsync();

            var response = await PostEditAsync(admin, self.Id,
                self.FirstName, self.LastName, AppRoles.Manager, self.PhoneNumber ?? "");

            AssertRedirectsTo(response, "/Users");
            (await GetRolesAsync(Fixture.AdminEmail)).Should().ContainSingle().Which.Should().Be(AppRoles.Admin);
        }

        [Fact]
        public async Task Edit_AnotherAdmin_PostedRoleIsIgnored()
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Admin);
            using var admin = await LoginAsAdminAsync();

            var response = await PostEditAsync(admin, id, "Other", "Admin", AppRoles.Secretary);

            AssertRedirectsTo(response, "/Users");
            (await GetRolesAsync(email)).Should().ContainSingle().Which.Should().Be(AppRoles.Admin);
        }

        [Fact]
        public async Task EditPage_ProtectedAccountHasLockedRole_RegularAccountOffersOnlyAssignableRoles()
        {
            var adminId = (await FindUserAsync(Fixture.AdminEmail))!.Id;
            var (userId, _) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var admin = await LoginAsAdminAsync();

            var protectedHtml = await (await admin.GetAsync($"/Users/Edit/{adminId}")).Content.ReadAsStringAsync();
            protectedHtml.Should().Contain("Administrator roles can't be changed here");

            var regularHtml = await (await admin.GetAsync($"/Users/Edit/{userId}")).Content.ReadAsStringAsync();
            regularHtml.Should().Contain("value=\"Manager\"")
                .And.Contain("value=\"Secretary\"")
                .And.NotContain("value=\"Admin\"");
        }

        [Fact]
        public async Task ChangeRole_AppliesToActiveSession_OnNextRequest_WithoutLoggingOut()
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var user = await LoginAsync(email, KnownPassword);
            (await RoleShownAsync(user)).Should().Be(AppRoles.Secretary);

            using var admin = await LoginAsAdminAsync();
            AssertRedirectsTo(await PostEditAsync(admin, id, "Direct", "User", AppRoles.Manager), "/Users");

            (await RoleShownAsync(user)).Should().Be(AppRoles.Manager);
            (await GetRolesAsync(email)).Should().ContainSingle().Which.Should().Be(AppRoles.Manager);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Deactivate_EndsUserSession_OnNextRequest(bool rememberMe)
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var user = await LoginAsync(email, KnownPassword, rememberMe);
            (await user.GetAsync("/Dashboard")).StatusCode.Should().Be(HttpStatusCode.OK);

            using var admin = await LoginAsAdminAsync();
            AssertRedirectsTo(await PostAsAdminAsync(admin, $"/Users/Deactivate/{id}"), "/Users");

            AssertRedirectsTo(await user.GetAsync("/Dashboard"), "/Account/Login");
            (await FindUserAsync(email))!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task Deactivate_UserCannotLogInAgain()
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Manager);
            using var admin = await LoginAsAdminAsync();
            await PostAsAdminAsync(admin, $"/Users/Deactivate/{id}");

            using var anon = CreateClient();
            var token = await GetAntiForgeryTokenAsync(anon, "/Account/Login");
            var response = await anon.PostAsync("/Account/Login", Form(
                ("Email", email), ("Password", KnownPassword), ("Remember", "false"),
                ("__RequestVerificationToken", token)));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Contain("deactivated");
        }

        [Fact]
        public async Task Deactivate_RotatesSecurityStamp_AndInvalidatesOutstandingResetToken()
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Manager);

            var before = await WithUserManagerAsync(async um =>
            {
                var u = await um.FindByIdAsync(id);
                u.Should().NotBeNull($"user {id} ({email}) was just created");

                var token = await um.GeneratePasswordResetTokenAsync(u!);
                return new { Stamp = u.SecurityStamp, Token = token };
            });

            using var admin = await LoginAsAdminAsync();
            var response = await PostAsAdminAsync(admin, $"/Users/Deactivate/{id}");
            AssertRedirectsTo(response, "/Users");

            var after = await WithUserManagerAsync(async um =>
            {
                var u = await um.FindByIdAsync(id);
                u.Should().NotBeNull("deactivation must not delete the user");

                var reset = await um.ResetPasswordAsync(u!, before.Token, NewPassword);
                return new { Stamp = u.SecurityStamp, reset.Succeeded };
            });

            after.Stamp.Should().NotBe(before.Stamp);
            after.Succeeded.Should().BeFalse("a token issued before deactivation must not survive it");
        }

        [Fact]
        public async Task Deactivate_PendingUser_KillsInvitation_ReactivateAndResendIssuesWorkingLink()
        {
            var email = await AdminCreatesUserAsync(AppRoles.Secretary);
            var id = (await FindUserAsync(email))!.Id;
            var oldLink = InvitationLinksFor(email).Single();
            using var admin = await LoginAsAdminAsync();
            using var anon = CreateClient();

            await PostAsAdminAsync(admin, $"/Users/Deactivate/{id}");
            AssertRedirectsTo(await anon.GetAsync(oldLink.RelativeUrl), "/Account/Login");

            await PostAsAdminAsync(admin, $"/Users/Reactivate/{id}");
            AssertRedirectsTo(await anon.GetAsync(oldLink.RelativeUrl), "/Account/Login");

            await PostAsAdminAsync(admin, $"/Users/ResendInvitation/{id}");
            var newLink = InvitationLinksFor(email).Last();

            AssertRedirectsTo(await PostSetPasswordAsync(anon, newLink, NewPassword, NewPassword), "/Account/Login");
            using var _ = await LoginAsync(email, NewPassword);
        }

        [Fact]
        public async Task Deactivate_Self_IsRefused_AndAdminStaysSignedIn()
        {
            var adminId = (await FindUserAsync(Fixture.AdminEmail))!.Id;
            using var admin = await LoginAsAdminAsync();

            AssertRedirectsTo(await PostAsAdminAsync(admin, $"/Users/Deactivate/{adminId}"), "/Users");

            (await FindUserAsync(Fixture.AdminEmail))!.IsActive.Should().BeTrue();
            (await admin.GetAsync("/Users")).StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Deactivate_AnotherAdmin_IsRefused()
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Admin);
            using var admin = await LoginAsAdminAsync();

            await PostAsAdminAsync(admin, $"/Users/Deactivate/{id}");

            (await FindUserAsync(email))!.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task Deactivate_PostedByNonAdmin_IsDenied()
        {
            var (_, secretaryEmail) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            var (managerId, managerEmail) = await CreateUserDirectlyAsync(AppRoles.Manager);
            using var secretary = await LoginAsync(secretaryEmail, KnownPassword);

            var token = await GetAntiForgeryTokenAsync(secretary, "/Dashboard");
            var response = await secretary.PostAsync($"/Users/Deactivate/{managerId}",
                Form(("__RequestVerificationToken", token)));

            AssertRedirectsTo(response, "/Account/AccessDenied");
            (await FindUserAsync(managerEmail))!.IsActive!.Should().BeTrue();
        }

        [Fact]
        public async Task Index_ShowsDeactivateForRegularUsers_ButNotForOwnAccount()
        {
            var adminId = (await FindUserAsync(Fixture.AdminEmail))!.Id;
            var (userId, _) = await CreateUserDirectlyAsync(AppRoles.Manager);
            using var admin = await LoginAsAdminAsync();

            var html = await (await admin.GetAsync("/Users")).Content.ReadAsStringAsync();

            html.Should().Contain($"/Users/Deactivate/{userId}")
                .And.NotContain($"/Users/Deactivate/{adminId}");
        }

        [Fact]
        public async Task Reactivate_RestoresLogin_AndClearsLockout()
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Secretary, isActive: false);
            await WithUserManagerAsync(async um =>
            {
                var u = (await um.FindByIdAsync(id))!;
                await um.SetLockoutEndDateAsync(u, DateTimeOffset.UtcNow.AddMinutes(15));
                await um.AccessFailedAsync(u);
            });
            using var admin = await LoginAsAdminAsync();

            AssertRedirectsTo(await PostAsAdminAsync(admin, $"/Users/Reactivate/{id}"), "/Users");

            var (isActive, lockedOut, failedCount) = await WithUserManagerAsync(async um =>
            {
                var u = (await um.FindByIdAsync(id))!;
                return (u.IsActive, await um.IsLockedOutAsync(u), u.AccessFailedCount);
            });

            isActive.Should().BeTrue();
            lockedOut.Should().BeFalse();
            failedCount.Should().Be(0);
        }

        [Fact]
        public async Task UnknownUserId_Returnss404_ForEditDeactivateAndReactivate()
        {
            var unknown = Guid.NewGuid().ToString();
            using var admin = await LoginAsAdminAsync();

            (await admin.GetAsync($"/Users/Edit/{unknown}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await PostAsAdminAsync(admin, $"/Users/Deactivate/{unknown}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await PostAsAdminAsync(admin, $"/Users/Reactivate/{unknown}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private static async Task<HttpResponseMessage> PostAsAdminAsync(
            HttpClient admin, string url, params (string Key, string Value)[] fields)
        {
            var token = await GetAntiForgeryTokenAsync(admin, "/Users");
            return await admin.PostAsync(url, Form([.. fields, ("__RequestVerificationToken", token)]));
        }

        private static Task<HttpResponseMessage> PostEditAsync(
            HttpClient admin, string id, string firstName, string lastName, string role, string phone = "") =>
            PostAsAdminAsync(admin, $"/Users/Edit/{id}",
                ("FirstName", firstName), ("LastName", lastName),
                ("PhoneNumber", phone), ("Role", role));

        private static async Task<string?> RoleShownAsync(HttpClient client)
        {
            var response = await client.GetAsync("/Dashboard");
            response.StatusCode.Should().Be(HttpStatusCode.OK, "the user should still be signed in");

            var html = await response.Content.ReadAsStringAsync();
            var match = Regex.Match(html, @"class=""dropdown-header"">\s*([^<]+?)\s*</h6>");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static void AssertRedirectsTo(HttpResponseMessage response, string path)
        {
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain(path);
        }
    }
}
