using AutomotiveDMS.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AutomotiveDMS.IntegrationTests.Fixtures
{
    public sealed record InvitationLink(string UserId, string Token, string RelativeUrl);
    public abstract class UserManagementTestBase : IAsyncLifetime
    {
        protected static readonly string KnownPassword = GeneratePassword();
        protected static readonly string NewPassword = GeneratePassword();

        private readonly List<string> _createdEmails = [];

        protected WebAppFixture Fixture { get; }

        protected UserManagementTestBase(WebAppFixture fixture) => Fixture = fixture;

        protected static string GeneratePassword() =>
            $"Aa1!{Convert.ToHexString(RandomNumberGenerator.GetBytes(12))}";

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => WithUserManagerAsync(async um =>
        {
            foreach (var email in _createdEmails)
            {
                var user = await um.FindByEmailAsync(email);
                if (user is not null) await um.DeleteAsync(user);
            }
        });

        protected HttpClient CreateClient() =>
            Fixture.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });

        protected Task<HttpClient> LoginAsAdminAsync() =>
            LoginAsync(Fixture.AdminEmail, Fixture.AdminPassword);

        protected async Task<HttpClient> LoginAsync(string email, string password)
        {
            var client = CreateClient();
            var token = await GetAntiForgeryTokenAsync(client, "/Account/Login");

            var response = await client.PostAsync("/Account/Login", Form(
                ("Email", email), ("Password", password), ("RememberMe", "false"),
                ("__RequestVerificationToken", token)));

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Redirect,
                $"login for '{email}' should succeed");

            return client;
        }

        protected string NewEmail()
        {
            var email = $"it-{Guid.NewGuid():N}@test.automotivedms.com";
            _createdEmails.Add(email);
            return email;
        }

        protected Task<(string Id, string Email)> CreateUserDirectlyAsync(
            string role, bool pending = false, bool isActive = true)
        {
            var email = NewEmail();

            return WithUserManagerAsync(async um =>
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = "Direct",
                    LastName = "User",
                    IsActive = isActive,
                    EmailConfirmed = true
                };

                var created = pending
                    ? await um.CreateAsync(user)
                    : await um.CreateAsync(user, KnownPassword);
                created.Succeeded.Should().BeTrue(string.Join(", ", created.Errors.Select(e => e.Description)));

                (await um.AddToRoleAsync(user, role)).Succeeded.Should().BeTrue();
                return (user.Id, email);
            });
        }

        protected async Task<string> AdminCreatesUserAsync(string role)
        {
            using var admin = await LoginAsAdminAsync();
            var email = NewEmail();

            var response = await PostCreateUserAsync(admin, email, role);
            await AssertRedirectAsync(response);
            return email;
        }

        protected async Task<HttpResponseMessage> PostCreateUserAsync(
            HttpClient admin, string email, string role,
            string firstName = "Test", string lastName = "User")
        {
            var token = await GetAntiForgeryTokenAsync(admin, "/Users/Create");

            return await admin.PostAsync("/Users/Create", Form(
                ("FirstName", firstName), ("LastName", lastName), 
                ("Email", email), ("PhoneNumber", ""), ("Role", role),
                ("__RequestVerificationToken", token)));
        }

        protected Task<ApplicationUser?> FindUserAsync(string email) =>
            WithUserManagerAsync(um => um.FindByEmailAsync(email));

        protected Task<IList<string>> GetRolesAsync(string email) =>
            WithUserManagerAsync(async um => await um.GetRolesAsync((await um.FindByEmailAsync(email))!));

        protected IReadOnlyList<InvitationLink> InvitationLinksFor(string email) =>
            Fixture.Emails.SentTo(email)
                .Select(m => Regex.Match(m.PlainTextBody, @"(/Account/SetPassword\?\S+)"))
                .Where(m => m.Success)
                .Select(m =>
                {
                    var relative = m.Groups[1].Value;
                    var query = QueryHelpers.ParseQuery(relative[(relative.IndexOf('?') + 1)..]);
                    return new InvitationLink(query["userId"]!, query["token"]!, relative);
                })
                .ToList();

        protected async Task<HttpResponseMessage> PostSetPasswordAsync(
            HttpClient client, InvitationLink link, string password, string confirm)
        {
            var token = await GetAntiForgeryTokenAsync(client, "/Account/Login");

            return await client.PostAsync("/Account/SetPassword", Form(
                ("UserId", link.UserId), ("Token", link.Token),
                ("Password", password), ("ConfirmPassword", confirm),
                ("__RequestVerificationToken", token)));
        }

        protected static FormUrlEncodedContent Form(params (string Key, string Value)[] fields) =>
            new(fields.Select(f => KeyValuePair.Create(f.Key, f.Value)));

        protected static async Task<string> GetAntiForgeryTokenAsync(HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK,
                $"GET {url} should render a page containing a form");

            var html = await response.Content.ReadAsStringAsync();
            var match = Regex.Match(html,
                @"<input[^>]+name=""__RequestVerificationToken""[^>]+value=""([^""]+)""");

            match.Success.Should().BeTrue($"an anti-forgery token must be present on {url}");
            return match.Groups[1].Value;
        }
        protected async Task WithUserManagerAsync(Func<UserManager<ApplicationUser>, Task> action)
        {
            using var scope = Fixture.Services.CreateScope();
            await action(scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        }

        protected async Task<T> WithUserManagerAsync<T>(Func<UserManager<ApplicationUser>, Task<T>> action)
        {
            using var scope = Fixture.Services.CreateScope();
            return await action(scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        }

        protected static async Task AssertRedirectAsync(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Redirect) return;

            var html = await response.Content.ReadAsStringAsync();
            var errors = Regex.Matches(html,
                    @"(?:validation-summary-errors|field-validation-error)[^>]*>(.*?)</(?:div|span)>",
                    RegexOptions.Singleline)
                .Select(m => Regex.Replace(m.Groups[1].Value, "<.*?>", " ").Trim())
                .Where(s => s.Length > 0)
                .ToList();

            throw new Xunit.Sdk.XunitException(
                $"Expected a redirect but got {(int)response.StatusCode}. " +
                $"Form errors: {(errors.Count > 0 ? string.Join(" | ", errors) : "(none found in HTML)")}");
        }
    }
}
