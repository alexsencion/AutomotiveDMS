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
    public class UsersControllerTests(WebAppFixture fixture)
        : UserManagementTestBase(fixture)
    {
        [Fact]
        public async Task Index_Anonymous_RedirectsToLogin()
        {
            using var client = CreateClient();

            var response = await client.GetAsync("/Users");

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("/Account/Login");
        }

        [Theory]
        [InlineData(AppRoles.Manager)]
        [InlineData(AppRoles.Secretary)]
        public async Task Index_NonAdmin_RedirectsToAccessDenied(string role)
        {
            var (_, email) = await CreateUserDirectlyAsync(role);
            using var client = await LoginAsync(email, KnownPassword);

            var response = await client.GetAsync($"/Users");

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("/Account/AccessDenied");
        }

        [Fact]
        public async Task Index_Admin_ListUsers()
        {
            using var admin = await LoginAsAdminAsync();

            var response = await admin.GetAsync("/Users");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Contain(Fixture.AdminEmail);
        }

        [Fact]
        public async Task Create_PostedByNonAdmin_IsDenied_AndCreatesNothing()
        {
            var (_, secretaryEmail) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var client = await LoginAsync(secretaryEmail, KnownPassword);

            var token = await GetAntiForgeryTokenAsync(client, "/Dashboard");
            var target = NewEmail();

            var response = await client.PostAsync("/Users/Create", Form(
                ("FirstName", "Sneaky"), ("LastName", "User"), ("Email", target), 
                ("Role", AppRoles.Manager), ("__RequestVerificationToken", token)));

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("/Account/AccessDenied");
            (await FindUserAsync(target)).Should().BeNull();
        }

        [Theory]
        [InlineData(AppRoles.Manager)]
        [InlineData(AppRoles.Secretary)]
        public async Task Create_ValidUser_CreatesPendingAccountWithRole_AndSendsInvitation(string role)
        {
            using var admin = await LoginAsAdminAsync();
            var email = NewEmail();

            var response = await PostCreateUserAsync(admin, email, role);

            await AssertRedirectAsync(response);
            response.Headers.Location!.ToString().Should().EndWith("/Users");

            var user = await FindUserAsync(email);
            user.Should().NotBeNull();
            user!.PasswordHash.Should().BeNull("the user sets their own password via the invitation");
            user.IsActive.Should().BeTrue();
            user.EmailConfirmed.Should().BeTrue();

            (await GetRolesAsync(email)).Should().ContainSingle().Which.Should().Be(role);

            Fixture.Emails.SentTo(email).Should().ContainSingle()
                .Which.Subject.Should().Be("Your Automotive DMS account");
            InvitationLinksFor(email).Should().ContainSingle()
                .Which.UserId.Should().Be(user.Id);
        }

        [Theory]
        [InlineData(AppRoles.Admin)]
        [InlineData("SuperUser")]
        public async Task Create_WithNonAssignableRole_IsRejected_AndCreatesNothing(string role)
        {
            using var admin = await LoginAsAdminAsync();
            var email = NewEmail();

            var response = await PostCreateUserAsync(admin, email, role);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Contain("Select a valid role.");
            (await FindUserAsync(email)).Should().BeNull();
            Fixture.Emails.SentTo(email).Should().BeEmpty();
        }

        [Fact]
        public async Task Create_WithMissingFields_ShowsAllErrors_AndCreatesNothing()
        {
            using var admin = await LoginAsAdminAsync();

            var response = await PostCreateUserAsync(admin, email: "", role: "", firstName: "", lastName: "");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("First name is required")
                .And.Contain("Last name is required")
                .And.Contain("Email is required")
                .And.Contain("Select a role");
        }

        [Fact]
        public async Task Create_WithExistingEmail_DifferentCase_IsRejected()
        {
            var (_, existing) = await CreateUserDirectlyAsync(AppRoles.Secretary);
            using var admin = await LoginAsAdminAsync();

            var response = await PostCreateUserAsync(admin, existing.ToUpperInvariant(), AppRoles.Manager);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var html = await response.Content.ReadAsStringAsync();
            html.Should().NotMatchRegex(@"field-validation-error""\s+data-valmsg-for=""PhoneNumber""");
            html.Should().Contain("already exists");

            var count = await WithUserManagerAsync(um =>
                um.Users.CountAsync(u => u.NormalizedEmail == existing.ToUpperInvariant()));
            count.Should().Be(1);
            Fixture.Emails.SentTo(existing).Should().BeEmpty();
        }

        [Fact]
        public async Task Create_TrimsWhitespaceFromNamesAndEmail()
        {
            using var admin = await LoginAsAdminAsync();
            var email = NewEmail();

           var response = await PostCreateUserAsync(admin, $"   {email}    ", AppRoles.Secretary,
                firstName: "  Ana  ", lastName: "  Perez ");

            var body = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.Redirect, body);

            var user = await FindUserAsync(email);
            user.Should().NotBeNull();
            user!.Email.Should().Be(email);
            user.UserName.Should().Be(email);
            user.FirstName.Should().Be("Ana");
            user.LastName.Should().Be("Perez");
        }

        [Fact]
        public async Task Index_ShowsInvitationPending_ForUserWithoutPassword()
        {
            var email = await AdminCreatesUserAsync(AppRoles.Manager);
            using var admin = await LoginAsAdminAsync();

            var html = await (await admin.GetAsync("/Users")).Content.ReadAsStringAsync();

            html.Should().Contain(email).And.Contain("Invitation pending");
        }
    }
}
