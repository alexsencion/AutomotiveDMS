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
    public class InvitationFlowTests(WebAppFixture fixture)
        : UserManagementTestBase(fixture)
    {
        [Fact]
        public async Task SetPassword_ValidLink_ShowsForm()
        {
            var email = await AdminCreatesUserAsync(AppRoles.Secretary);
            var link = InvitationLinksFor(email).Single();
            using var anon = CreateClient();

            var response = await anon.GetAsync(link.RelativeUrl);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Contain("Welcome to Automotive DMS");
        }

        [Fact]
        public async Task SetPassword_ValidLink_SetsPassword_AndUserCanLogIn()
        {
            var email = await AdminCreatesUserAsync(AppRoles.Secretary);
            var link = InvitationLinksFor(email).Single();
            using var anon = CreateClient();

            var response = await PostSetPasswordAsync(anon, link, NewPassword, NewPassword);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("/Account/Login");

            using var userClient = await LoginAsync(email, NewPassword);
        }

        [Fact]
        public async Task SetPassword_LinkIsSingleUse()
        {
            var email = await AdminCreatesUserAsync(AppRoles.Manager);
            var link = InvitationLinksFor(email).Single();
            using var anon = CreateClient();
            await PostSetPasswordAsync(anon, link, NewPassword, NewPassword);

            var get = await anon.GetAsync(link.RelativeUrl);
            get.StatusCode.Should().Be(HttpStatusCode.Redirect);
            get.Headers.Location!.ToString().Should().Contain($"/Account/Login");

            var another = GeneratePassword();
            var post = await PostSetPasswordAsync(anon, link, another, another);
            post.StatusCode.Should().Be(HttpStatusCode.OK);
            (await post.Content.ReadAsStringAsync()).Should().ContainEquivalentOf("already used");
        }

        [Fact]
        public async Task SetPassword_TamperedToken_RedirectsToLogin()
        {
            var email = await AdminCreatesUserAsync(AppRoles.Secretary);
            var link = InvitationLinksFor(email).Single();
            using var anon = CreateClient();

            var response = await anon.GetAsync(
                $"/Account/SetPassword?userId={link.UserId}&token=not-a-real-token");

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("/Account/Login");
        }

        [Fact]
        public async Task SetPassword_WeakPassword_ShowsPolicyErrors_AndAccountStaysPending()
        {
            var email = await AdminCreatesUserAsync(AppRoles.Secretary);
            var link = InvitationLinksFor(email).Single();
            using var anon = CreateClient();

            var response = await PostSetPasswordAsync(anon, link, "abc", "abc");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().ContainEquivalentOf("at least 8 characters");
            (await FindUserAsync(email))!.PasswordHash.Should().BeNull();
        }

        [Fact]
        public async Task SetPassword_DeactivatedUser_LinkIsRejected()
        {
            var email = await AdminCreatesUserAsync(AppRoles.Secretary);
            var link = InvitationLinksFor(email).Single();

            await WithUserManagerAsync(async um =>
            {
                var user = await um.FindByEmailAsync(email);
                user!.IsActive = false;
                await um.UpdateAsync(user);
            });

            using var anon = CreateClient();
            var response = await anon.GetAsync(link.RelativeUrl);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("/Account/Login");
        }

        [Fact]
        public async Task PendingUser_CannotLogIn()
        {
            var email = await AdminCreatesUserAsync(AppRoles.Manager);
            using var anon = CreateClient();
            var token = await GetAntiForgeryTokenAsync(anon, "/Account/Login");

            var response = await anon.PostAsync("/Account/Login", Form(
                ("Email", email), ("Password", KnownPassword), ("RememberMe", "false"), 
                ("__RequestVerificationToken", token)));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Contain("Invalid email or password");
        }

        [Fact]
        public async Task InvitationToken_IsRejectedByPasswordResetEndpoint()
        {
            var email = await AdminCreatesUserAsync(AppRoles.Secretary);
            var link = InvitationLinksFor(email).Single();
            using var anon = CreateClient();
            var token = await GetAntiForgeryTokenAsync(anon, "/Account/Login");

            var response = await anon.PostAsync("/Account/ResetPassword", Form(
                ("UserId", link.UserId), ("Token", link.Token),
                ("Password", NewPassword), ("ConfirmPassword", NewPassword),
                ("__RequestVerificationToken", token)));
        }

        [Fact]
        public async Task PendingUser_CanSetPasswordThroughPasswordResetFallback()
        {
            var (_, email) = await CreateUserDirectlyAsync(AppRoles.Secretary, pending: true);

            var succeeded = await WithUserManagerAsync(async um =>
            {
                var user = (await um.FindByEmailAsync(email))!;
                var token = await um.GeneratePasswordResetTokenAsync(user);
                var reset = await um.ResetPasswordAsync(user, token, NewPassword);
                return reset.Succeeded && await um.CheckPasswordAsync(user, NewPassword);
            });

            succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task ResendInvitation_SendsNewLink_AndUsingOneInvalidatesTheOther()
        {
            var email = await AdminCreatesUserAsync(AppRoles.Manager);
            var userId = (await FindUserAsync(email))!.Id;
            using var admin = await LoginAsAdminAsync();

            var resend = await PostResendAsync(admin, userId);
            resend.StatusCode.Should().Be(HttpStatusCode.Redirect);

            var links = InvitationLinksFor(email);
            links.Should().HaveCount(2);

            using var anon = CreateClient();
            (await PostSetPasswordAsync(anon, links[1], NewPassword, NewPassword))
                .StatusCode.Should().Be(HttpStatusCode.Redirect);

            var stale = await anon.GetAsync(links[0].RelativeUrl);
            stale.StatusCode.Should().Be(HttpStatusCode.Redirect);
            stale.Headers.Location!.ToString().Should().Contain("/Account/Login");
        }

        [Fact]
        public async Task ResendInvitation_UserWithPassword_SendsNothing()
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var admin = await LoginAsAdminAsync();

            var response = await PostResendAsync(admin, id);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            Fixture.Emails.SentTo(email).Should().BeEmpty();
        }

        [Fact]
        public async Task ResendInvitation_InactiveUser_SendsNothing()
        {
            var (id, email) = await CreateUserDirectlyAsync(AppRoles.Secretary, pending: true, isActive: false);
            using var admin = await LoginAsAdminAsync();

            var response = await PostResendAsync(admin, id);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            Fixture.Emails.SentTo(email).Should().BeEmpty();
        }

        [Fact]
        public async Task ResendInvitation_UnknownUser_Returns404()
        {
            using var admin = await LoginAsAdminAsync();

            var response = await PostResendAsync(admin, Guid.NewGuid().ToString());

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }



        private static async Task<HttpResponseMessage> PostResendAsync(HttpClient admin, string userId)
        {
            var token = await GetAntiForgeryTokenAsync(admin, "/Users");
            return await admin.PostAsync($"/Users/ResendInvitation/{userId}",
                Form(("__RequestVerificationToken", token)));
        }
    }
}
