using AutomotiveDMS.Infrastructure.Data;
using AutomotiveDMS.Infrastructure.Identity;
using AutomotiveDMS.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Twilio.Jwt.AccessToken;

namespace AutomotiveDMS.IntegrationTests.Controllers
{
    [Collection("Integration Tests Collection")]
    public class AccountControllerTests
    {
        private readonly WebAppFixture _fixture;
        private readonly HttpClient _client;

        public AccountControllerTests(WebAppFixture fixture)
        {
            _fixture = fixture;

            _client = _fixture.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetLogin_ReturnsLoginPage()
        {
            var response = await _client.GetAsync("/Account/Login");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Sign in to your account");
        }

        [Fact]
        public async Task GetLogin_WhenAlreadyAuthenticated_RedirectsToDashboard()
        {
            var authenticatedClient = await GetAuthenticatedClientAsync();

            var response = await authenticatedClient.GetAsync("Account/Login");

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().NotContain("Login");
        }

        [Fact]
        public async Task PostLogin_WithValidCredentials_RedirectsToDashboard()
        {
            var token = await GetAntiForgeryTokenAsync("/Account/Login");

            var response = await _client.PostAsync("/Account/Login", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["Email"] = _fixture.AdminEmail,
                    ["Password"] = _fixture.AdminPassword,
                    ["RememberMe"] = "false",
                    ["__RequestVerificationToken"] = token
                }));

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().NotContain("Login");
        }

        [Fact]
        public async Task PostLogin_WithInvalidPassword_ReturnsLoginViewWithError()
        {
            using var scope = _fixture.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var email = $"wrongpasstest-{Guid.NewGuid():N}@automotivedms.com";

            var testUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = "WrongPass",
                LastName = "Test",
                IsActive = true,
                EmailConfirmed = true,
                CreatedDate = DateTime.UtcNow
            };
            var createdResult = await userManager.CreateAsync(testUser, "Correct@12345!");
            createdResult.Succeeded.Should().BeTrue(
                "test setup requires the user to be created: " +
                string.Join(", ", createdResult.Errors.Select(e => e.Description)));

            try
            {
                var token = await GetAntiForgeryTokenAsync("/Account/Login");

                var response = await _client.PostAsync("/Account/Login", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["Email"] = email,
                    ["Password"] = "WrongPassword!",
                    ["RememberMe"] = "false",
                    ["__RequestVerificationToken"] = token
                }));

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Invalid email or password");
            }
            finally
            {
                await userManager.DeleteAsync(testUser);
            }
        }

        [Fact]
        public async Task PostLogin_WithUnknownEmail_ReturnsLoginViewWithGenericError()
        {
            var token = await GetAntiForgeryTokenAsync("/Account/Login");

            var response = await _client.PostAsync("/Account/Login", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["Email"] = "nobody@automotivedms.com",
                    ["Password"] = "SomePassword!",
                    ["RememberMe"] = "false",
                    ["__RequestVerificationToken"] = token
                }));

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Invalid email or password");
            content.Should().NotContain("Email not found");
        }

        [Fact]
        public async Task PostLogin_WithInactiveUser_ReturnsDeactivatedMessage()
        {
            using var scope = _fixture.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var inactiveUser = new ApplicationUser
            {
                UserName = "inactive@automotivedms.com",
                Email = "inactive@automotivedms.com",
                FirstName = "Inactive",
                LastName = "User",
                IsActive = false,
                EmailConfirmed = true,
                CreatedDate = DateTime.UtcNow
            };
            await userManager.CreateAsync(inactiveUser, "Test@12345!");

            var token = await GetAntiForgeryTokenAsync("/Account/Login");

            var response = await _client.PostAsync("/Account/Login", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["Email"] = "inactive@automotivedms.com",
                    ["Password"] = "Test@12345!",
                    ["RememberMe"] = "false",
                    ["__RequestVerificationToken"] = token
                }));

            await userManager.DeleteAsync(inactiveUser);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("deactivated");
        }

        [Fact]
        public async Task PostLogin_WithEmptyEmail_ReturnsValidationError()
        {
            var token = await GetAntiForgeryTokenAsync("/Account/Login");

            var response = await _client.PostAsync("/Account/Login", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["Email"] = "",
                    ["Password"] = "Test@12345!",
                    ["RememberMe"] = "false",
                    ["__RequestVerificationToken"] = token
                }));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Email is required");
        }

        [Fact]
        public async Task PostLogin_SetsLastLoginDate_AfterSuccessfulLogin()
        {
            var token = await GetAntiForgeryTokenAsync("/Account/Login");

            var response = await _client.PostAsync("/Account/Login", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["Email"] = _fixture.AdminEmail,
                    ["Password"] = _fixture.AdminPassword,
                    ["RememberMe"] = "false",
                    ["__RequestVerificationToken"] = token
                }));

            response.StatusCode.Should().Be(HttpStatusCode.Redirect,
                because: "a successful login should redirect to Dashboard");

            // await Task.Delay(TimeSpan.FromMilliseconds(500));

            using var scope = _fixture.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(_fixture.AdminEmail);

            user.Should().NotBeNull(because: "the seeded admin user must exist for this test to be meaningful");

            user!.LastLoginDate.Should().NotBeNull(because: "LastLoginDate must be set on every successful login");
            user.LastLoginDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        }

        [Fact]
        public async Task GetAccessDenied_ReturnsAccessDeniedPage()
        {
            var response = await _client.GetAsync("/Account/AccessDenied");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Access Denied");
        }

        private async Task<HttpClient> GetAuthenticatedClientAsync()
        {
            var client = _fixture.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var token = await GetAntiForgeryTokenAsync("/Account/Login", client);

           var loginResponse = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["Email"] = _fixture.AdminEmail,
                    ["Password"] = _fixture.AdminPassword,
                    ["RememberMe"] = "false",
                    ["__RequestVerificationToken"] = token
                }));

            if (loginResponse.StatusCode != HttpStatusCode.Redirect)
            {
                var body = await loginResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"GetAuthenticatedClientAsync: login did not succeed. " +
                    $"Status: {loginResponse.StatusCode}. Body snippet: " +
                    $"{body[..Math.Min(300, body.Length)]}");
            }

            return client;
        }

        private async Task<string> GetAntiForgeryTokenAsync(
            string url,
            HttpClient? client = null)
        {
            client ??= _client;
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            return ExtractAntiForgeryToken(content);
        }

        private static string ExtractAntiForgeryToken(string htmlContent)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                htmlContent,
                @"<input[^>]+name=""__RequestVerificationToken""[^>]+value=""([^""]+)""");

            if (!match.Success)
                throw new InvalidOperationException(
                    "Anti-forgery token not found in page." +
                    "Ensure the view includes @Html.AntiForgeryToken() or asp-action tag helpers.");

            return match.Groups[1].Value;
        }
    }
}
