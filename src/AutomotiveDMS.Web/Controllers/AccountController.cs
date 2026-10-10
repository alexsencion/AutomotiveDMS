using AutomotiveDMS.Application.Interfaces.Services;
using AutomotiveDMS.Infrastructure.Identity;
using AutomotiveDMS.Web.Emails;
using AutomotiveDMS.Web.Extensions;
using AutomotiveDMS.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Text;

namespace AutomotiveDMS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly DataProtectionTokenProviderOptions _tokenOptions;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IConfiguration configuration,
            IOptions<DataProtectionTokenProviderOptions> tokenOptions,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
            _tokenOptions = tokenOptions.Value;
            _logger = logger;
        }

        private const string InvalidLinkMessage =
            "This reset link is invalid or has expired. Please request a new one.";

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl ?? Url.Content("~/")
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            model.ReturnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                userName: model.Email,
                password: model.Password,
                isPersistent: model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "User '{Email}' logged in successfully", model.Email);

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user is not null)
                {
                    user.LastLoginDate = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }

                if (Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Dashboard");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning(
                    "User '{Email}' is locked out", model.Email);

                ModelState.AddModelError(string.Empty,
                    "This user has been locked due to multiple failed login attempts." +
                    "Please try again in 15 minutes.");

                return View(model);
            }

            if (result.IsNotAllowed)
            {
                _logger.LogWarning(
                    "Inactive user '{Email}' attempted login", model.Email);

                ModelState.AddModelError(string.Empty,
                    "This user has been deactivated. Please contact the administrator.");

                return View(model);
            }

            _logger.LogWarning(
                "Failed login attempt for '{Email}'", model.Email);

            ModelState.AddModelError(string.Empty,
                "Invalid email or password.");

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicies.PassowordReset)]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !user.IsActive)
            {
                _logger.LogInformation(
                    "Password reset requested for an unknown or inactive account");
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var email = PasswordResetEmail.Create(
                recepientEmail: user.Email!,
                recipientName: user.FullName,
                resetLink: BuildResetLink(user.Id, encodedToken),
                validFor: _tokenOptions.TokenLifespan);

            var sent = await _emailService.SendAsync(email, cancellationToken);

            if (sent)
            {
                _logger.LogInformation("Password reset email sent to user {UserId}", user.Id);
            }
            else
            {
                _logger.LogError("Password reset email FAILED for user {UserId}", user.Id);
            }

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation() => View();

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string? userId, string? token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                TempData["ErrorMessage"] = InvalidLinkMessage;
                return RedirectToAction(nameof(Login));
            }

            Response.Headers["Referrer-Policy"] = "no-referre";
            Response.Headers.CacheControl = "no-store";

            var user = await _userManager.FindByIdAsync(userId);

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            }
            catch (FormatException)
            {
                TempData["ErrorMessage"] = InvalidLinkMessage;
                return RedirectToAction(nameof(Login));
            }

            var isValid = user is { IsActive: true } &&
                await _userManager.VerifyUserTokenAsync(
                    user,
                    _userManager.Options.Tokens.PasswordResetTokenProvider,
                    UserManager<ApplicationUser>.ResetPasswordTokenPurpose,
                    decodedToken);

            if (!isValid)
            {
                TempData["ErrorMessage"] = InvalidLinkMessage;
                return RedirectToAction(nameof(Login));
            }

            return View(new ResetPasswordViewModel { UserId = userId, Token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user is null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, InvalidLinkMessage);
                return View(model);
            }

            string token;
            try
            {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            }
            catch (FormatException)
            {
                ModelState.AddModelError(string.Empty, InvalidLinkMessage);
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

            if (!result.Succeeded)
            {
                var invalidTokenCode = _userManager.ErrorDescriber.InvalidToken().Code;

                if (result.Errors.Any(e => e.Code == invalidTokenCode))
                {
                    ModelState.AddModelError(string.Empty, InvalidLinkMessage);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(nameof(model.Password), error.Description);
                    }
                }

                return View(model);
            }

            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);

            _logger.LogInformation("Password reset completed for user {UserId}", user.Id);

            TempData["SuccessMessage"] =
                "Your password has been reset. Sign in with your new password.";

            return RedirectToAction(nameof(Login));
        }


        private string BuildResetLink(string userId, string encodedToken)
        {
            var baseUrl = _configuration["App:Domain"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("App:Domain is not configured.");

            var path = Url.Action(nameof(ResetPassword), "Account",
                new { userId, token = encodedToken })!;

            return $"{baseUrl}{path}";
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var email = User.Identity?.Name;

            await _signInManager.SignOutAsync();

            _logger.LogInformation(
                "User '{Email}' logged out", email);

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
