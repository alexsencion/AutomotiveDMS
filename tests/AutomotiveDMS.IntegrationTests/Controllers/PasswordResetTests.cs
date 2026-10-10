using AutomotiveDMS.Application.Interfaces.Services;
using AutomotiveDMS.Infrastructure.Identity;
using AutomotiveDMS.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AutomotiveDMS.IntegrationTests.Controllers
{
    [Collection("Integration Tests Collection")]
    public class PasswordResetTests : IAsyncLifetime
    {
        private const string Email = "resetuser@automotivedms.com";
        private const string OldPassword = "OldPass@2026!";
        private const string NewPassword = "NewPass@2026!";

        private readonly FakeEmailService _emails = new();
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PasswordResetTests(WebAppFixture fixture)
        {
            _factory = fixture.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IEmailService>();
                    services.AddSingleton<IEmailService>(_emails);
                });
            });

            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        public async Task DisposeAsync()
        {
            await DeleteUserAsync();
            _client.Dispose();
            await _factory.DisposeAsync();
        }

        public async Task InitializeAsync()
        {
            await DeleteUserAsync();

            await WithUserManagerAsync(async um =>
            {
                var user = new ApplicationUser
                {
                    UserName = Email,
                    Email = Email,
                    FirstName = "Reset",
                    LastName = "User",
                    IsActive = true,
                    EmailConfirmed = true,
                };
                (await um.CreateAsync(user, OldPassword)).Succeeded.Should().BeTrue();
            });
        }

        [Fact]
        public async Task GetForgotPassword_ReturnsPage()
        {
            var response = await _client.GetAsync("/Account/ForgotPassword");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync())
                .Should().Contain("Forgot your password?");
        }

        [Fact]
        public async Task PostForgotPassword_ActiveUser_SendsOneEmailWithResetLink()
        {
            var response = await PostForgotPasswordAsync(Email);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("ForgotPasswordConfirmation");

            _emails.Sent.Should().ContainSingle()
                .Which.PlainTextBody.Should().Contain("/Account/ResetPassword?");
        }

        [Fact]
        public async Task PostForgotPassword_UnknownEmail_SameRedirect_NoEmail()
        {
            var response = await PostForgotPasswordAsync("nobody@automotivedms.com");

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("ForgotPasswordConfirmation");
            _emails.Sent.Should().BeEmpty();
        }

        [Fact]
        public async Task PostForgotPassword_InactiveUser_SameRedirect_NoEmail()
        {
            await WithUserManagerAsync(async um =>
            {
                var user = await um.FindByEmailAsync(Email);
                user!.IsActive = false;
                await um.UpdateAsync(user);
            });

            var response = await PostForgotPasswordAsync(Email);

            response.Headers.Location!.ToString().Should().Contain("ForgotPasswordConfirmation");
            _emails.Sent.Should().BeEmpty();
        }

        [Fact]
        public async Task PostForgotPassword_SixthRequestFromSameIp_Returns429()
        {
            for (var i = 0; i < 5; i++)
            {
                (await PostForgotPasswordAsync("nobody@automotivedms.com"))
                    .StatusCode.Should().Be(HttpStatusCode.Redirect);
            }

            var sixth = await PostForgotPasswordAsync("nobody@automotivedms.com");

            sixth.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        }

        [Fact]
        public async Task ResetPassword_WithValidLink_ChangesPassword_AndNewPasswordWorks()
        {
            var (userId, token) = await RequestResetLinkAsync();

            var response = await PostResetPasswordAsync(userId, token, NewPassword, NewPassword);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("Login");

            await WithUserManagerAsync(async um =>
            {
                var user = await um.FindByEmailAsync(Email);
                (await um.CheckPasswordAsync(user!, NewPassword)).Should().BeTrue();
                (await um.CheckPasswordAsync(user!, OldPassword)).Should().BeFalse();
            });
        }

        [Fact]
        public async Task ResetPassword_TokenCannotBeReused()
        {
            var (userId, token) = await RequestResetLinkAsync();
            await PostResetPasswordAsync(userId, token, NewPassword, NewPassword);

            var second = await PostResetPasswordAsync(userId, token, "Another@2026!", "Another@2026!");

            second.StatusCode.Should().Be(HttpStatusCode.OK);
            (await second.Content.ReadAsStringAsync()).Should().Contain("invalid or has expired");
        }

        [Fact]
        public async Task ResetPassword_WithTamperedToken_ShowsInvalidLinkError()
        {
            var (userId, _) = await RequestResetLinkAsync();

            var response = await PostResetPasswordAsync(userId, "not-a-real-token", NewPassword, NewPassword);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Contain("invalid or has expired");
        }

        [Fact]
        public async Task ResetPassword_WithMismatchedConfirmation_ShowsValidationError()
        {
            var (userId, token) = await RequestResetLinkAsync();

            var response = await PostResetPasswordAsync(userId, token, NewPassword, "Different@2026!");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Contain("The passwords do not match");
        }

        [Fact]
        public async Task ResetPassword_ClearsExistingLockout()
        {
            await WithUserManagerAsync(async um =>
            {
                var user = await um.FindByEmailAsync(Email);
                await um.SetLockoutEndDateAsync(user!, DateTimeOffset.UtcNow.AddMinutes(15));
            });

            var (userId, token) = await RequestResetLinkAsync();
            await PostResetPasswordAsync(userId, token, NewPassword, NewPassword);

            await WithUserManagerAsync(async um =>
            {
                var user = await um.FindByEmailAsync(Email);
                (await um.IsLockedOutAsync(user!)).Should().BeFalse();
            });
        }

        private async Task<HttpResponseMessage> PostForgotPasswordAsync(string email)
        {
            var token = await GetAntiForgeryTokenAsync("/Account/ForgotPassword");
            return await _client.PostAsync("/Account/ForgotPassword", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["Email"] = email,
                    ["__RequestVerificationToken"] = token
                }));
        }

        [Fact]
        public async Task GetResetPassword_WithAlreadyUsedLink_RedirectsToLogin()
        {
            var (userId, token) = await RequestResetLinkAsync();
            await PostResetPasswordAsync(userId, token, NewPassword, NewPassword);

            var url = QueryHelpers.AddQueryString("/Account/ResetPassword",
                new Dictionary<string, string?> { ["UserId"] = userId, ["token"] = token });

            var response = await _client.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location!.ToString().Should().Contain("Login");
        }

        [Fact]
        public async Task GetResetPassword_WithValidLink_ShowsForm()
        {
            var (userId, token) = await RequestResetLinkAsync();

            var url = QueryHelpers.AddQueryString("/Account/ResetPassword", 
                new Dictionary<string, string?> { ["userId"] = userId, ["token"] = token });

            var response = await _client.GetAsync(url);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Contain("Choose a new password");
        }

        private async Task<(string UserId, string Token)> RequestResetLinkAsync()
        {
            await PostForgotPasswordAsync(Email);

            var body = _emails.Sent.Should().ContainSingle().Subject.PlainTextBody;
            var match = Regex.Match(body, @"/Account/ResetPassword\?(\S+)");
            match.Success.Should().BeTrue("the email must contaion a reset link");

            var query = QueryHelpers.ParseQuery(match.Groups[1].Value);
            return (query["userId"]!, query["token"]!);
        }

        private async Task<HttpResponseMessage> PostResetPasswordAsync(
            string userId, string token, string password, string confirm)
        {

            var antiForge = await GetAntiForgeryTokenAsync("/Account/ForgotPassword");

            return await _client.PostAsync("/Account/ResetPassword", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["UserId"] = userId,
                    ["Token"] = token,
                    ["Password"] = password,
                    ["ConfirmPassword"] = confirm,
                    ["__RequestVerificationToken"] = antiForge,
                }));
        }


        private async Task<string> GetAntiForgeryTokenAsync(string url)
        {
            var html = await (await _client.GetAsync(url)).Content.ReadAsStringAsync();
            var match = Regex.Match(html,
                @"<input[^>]+name=""__RequestVerificationToken""[^>]+value=""([^""]+)""");
            match.Success.Should().BeTrue($"an anti-forgery token must be present on {url}");
            return match.Groups[1].Value;
        }


        private async Task WithUserManagerAsync(Func<UserManager<ApplicationUser>, Task> action)
        {
            using var scope = _factory.Services.CreateScope();
            await action(scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        }

        private Task DeleteUserAsync() => WithUserManagerAsync(async um =>
        {
            var existing = await um.FindByEmailAsync(Email);
            if (existing is not null) await um.DeleteAsync(existing);
        });
    }
}
