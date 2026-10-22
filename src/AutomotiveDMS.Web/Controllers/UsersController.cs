using AutomotiveDMS.Application.Interfaces.Services;
using AutomotiveDMS.Infrastructure.Identity;
using AutomotiveDMS.Web.Emails;
using AutomotiveDMS.Web.Extensions;
using AutomotiveDMS.Web.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;

namespace AutomotiveDMS.Web.Controllers
{
    [Authorize(Policy = "CanManageUsers")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public readonly InvitationTokenProviderOptions _invitationOptions;
        public readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IConfiguration configuration,
            IOptions<InvitationTokenProviderOptions> invitationOptions,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
            _invitationOptions = invitationOptions.Value;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var users = await _userManager.Users
                .AsNoTracking()
                .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
                .ToListAsync();

            var items = new List<UserListItemViewModel>(users.Count);
            foreach (var u in users)
            {
                items.Add(new UserListItemViewModel
                {
                    Id = u.Id,
                    FullName = u.FirstName,
                    Email = u.Email ?? string.Empty,
                    Role = (await _userManager.GetRolesAsync(u)).FirstOrDefault() ?? "-",
                    IsActive = u.IsActive,
                    InvitationPending = u.PasswordHash is null,
                    LastLoginLocal = u.LastLoginDate is { } d ? DominicanTime.FromUtc(d) : null,
                });
            }

            return View(items);
        }

        [HttpGet]
        public IActionResult Create() =>
            View(new CreateUserViewModel { AvailableRoles = BuildRoleOptions() });


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateUserViewModel model, CancellationToken cancellationToken)
        {
            model.AvailableRoles = BuildRoleOptions();

            if (!AppRoles.AssignableOnCreation.Contains(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "Select a valid role.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Email.Trim();

            if (await _userManager.FindByEmailAsync(email) is not null)
            {
                ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                PhoneNumber = string.IsNullOrEmpty(model.PhoneNumber) ? null : model.PhoneNumber.Trim(),
                IsActive = true,
                EmailConfirmed = true,
                CreatedDate = DateTime.UtcNow,
            };

            var createdResult = await _userManager.CreateAsync(user);
            if (!createdResult.Succeeded)
            {
                foreach (var error in createdResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!roleResult.Succeeded)
            {
                var cleanup = await _userManager.DeleteAsync(user);
                _logger.LogError(
                    "Role '{Role}' assignment failed for new user {UserId}: {Errors}. Cleanup succeeded: {Cleanup}",
                    model.Role, user.Id,
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)),
                    cleanup.Succeeded);

                ModelState.AddModelError(string.Empty, "The user could not be created. Please try again.");
                return View(model);
            }

            _logger.LogInformation(
                "Admin {AdminId} created user {UserId} with role {Role}",
                User.FindFirstValue(ClaimTypes.NameIdentifier), user.Id, model.Role);

            var sent = await SendInvitationAsync(user, model.Role, cancellationToken);

            if (sent)
            {
                TempData["SuccessMessage"] =
                    $"{user.FullName} was created as {model.Role}. An invitation email was sent to {email}.";
            }
            else
            {
                TempData["WarningMessage"] =
                    $"{user.FullName} was created, but the invitation email could not be sent. " +
                    "Use \"Resend invitation\" to try again."; 
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendInvitation(string id, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            if (user.PasswordHash is not null)
            {
                TempData["WarningMessage"] = $"{user.FullName} has already set a password.";
                return RedirectToAction(nameof(Index));
            }

            if (!user.IsActive)
            {
                TempData["WarningMessage"] = $"{user.FullName} is deactivated. Reactivate the account first.";
                return RedirectToAction(nameof(Index));
            }

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty;
            var sent = await SendInvitationAsync(user, role, cancellationToken);

            TempData[sent ? "SuccessMessage" : "ErrorMessage"] = sent
                ? $"A new invitation was sent to {user.Email}."
                : "The invitation email could not be sent. Check the logs and try again.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> SendInvitationAsync(
            ApplicationUser user, string role, CancellationToken cancellationToken)
        {
            var token = await _userManager.GenerateUserTokenAsync(
                user, InvitationTokenProvider.ProviderName, InvitationTokenProvider.Purpose);

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var link = Url.AbsoluteActionFromAppDomain(_configuration,
                "SetPassword", "Account", new { userId = user.Id, token = encodedToken });

            var email = InvitationEmail.Create(
                user.Email!, user.FullName, role, link, _invitationOptions.TokenLifespan);

            var sent = await _emailService.SendAsync(email, cancellationToken);

            if (!sent)
            {
                _logger.LogError("Invitation email FAILED for user {UserId}", user.Id);
            }

            return sent;
        }

        private static IEnumerable<SelectListItem> BuildRoleOptions() =>
            AppRoles.AssignableOnCreation.Select(r => new SelectListItem(r, r));
    }
}
