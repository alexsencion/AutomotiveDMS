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
                var role = (await _userManager.GetRolesAsync(u)).FirstOrDefault() ?? "-";

                items.Add(new UserListItemViewModel
                {
                    Id = u.Id,
                    FullName = u.FirstName,
                    Email = u.Email ?? string.Empty,
                    Role = role,
                    IsActive = u.IsActive,
                    InvitationPending = u.PasswordHash is null,
                    LastLoginLocal = u.LastLoginDate is { } d ? DominicanTime.FromUtc(d) : null,
                    IsCurrentUser = u.Id == currentUserId,
                    IsProtected = u.Id == currentUserId || role == AppRoles.Admin
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

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            return View(await BuildEditModelAsync(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            var isProtected = await IsProtectedAsync(user);

            if (isProtected)
            {
                ModelState.Remove(nameof(model.Role))
;           }
            else if (!AppRoles.AssignableOnCreation.Contains(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "Select a valid role.");
            }

            if (!ModelState.IsValid)
            {
                return View(await BuildEditModelAsync(user, model));
            }

            user.FirstName = model.FirstName.Trim();
            user.LastName = model.LastName.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                foreach (var error in update.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(await BuildEditModelAsync(user, model));
            }

            if (!isProtected)
            {
                var currentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

                if (!string.Equals(currentRole, model.Role, StringComparison.Ordinal))
                {
                    if (!await ChangeRoleAsync(user, currentRole, model.Role))
                    {
                        ModelState.AddModelError(string.Empty,
                            "The details were saved, but the role could not be changed. Please try again");
                        return View(await BuildEditModelAsync(user, model));
                    }

                    _logger.LogInformation(
                        "Admin {AdminId} changed role of user {UserId} from {OldRole} to {NewRole}",
                        _userManager.GetUserId(User), user.Id, currentRole, model.Role);
                }
            }

            TempData["SuccessMessage"] = $"Changes to {user.FullName} were saved";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            if (await IsProtectedAsync(user))
            {
                TempData["ErrorMessage"] = $"Administrator accounts cannot be deactivated";
                return RedirectToAction(nameof(Index));
            }

            if (!user.IsActive)
            {
                TempData["WarningMessage"] = $"{user.FullName} is already deactivated";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = false;
            var result = await _userManager.UpdateSecurityStampAsync(user);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "The user could not be deactivated. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation("Admin {AdminId} deactivated user {UserId}",
                _userManager.GetUserId(User), user.Id);

            TempData["SuccessMessage"] = $"{user.FullName} was deactivated and signed out.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            if (user.IsActive)
            {
                TempData["WarningMessage"] = $"{user.FullName} is already active.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "The user could not be reactivated. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);

            _logger.LogInformation("Admin {AdminId} reactivated user {UserId}",
                _userManager.GetUserId(User), user.Id);

            TempData["SuccessMessage"] = user.PasswordHash is null
                ? $"{user.FullName} was reactivated. Their previous invitation link no longer works. Use \"Resend invitation\"."
                : $"{user.FullName} was reactivated and can sign in again.";

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

        private async Task<bool> IsProtectedAsync(ApplicationUser user) =>
            user.Id == _userManager.GetUserId(User) ||
            await _userManager.IsInRoleAsync(user, AppRoles.Admin);

        private async Task<bool> ChangeRoleAsync(ApplicationUser user, string? fromRole, string toRole)
        {
            if (fromRole is not null)
            {
                var removed = await _userManager.RemoveFromRoleAsync(user, fromRole);
                if (!removed.Succeeded)
                {
                    _logger.LogError("Could not remove role {Role} from user {UserId}", fromRole, user.Id);
                    return false;
                }
            }

            var added = await _userManager.AddToRoleAsync(user, toRole);
            if (added.Succeeded)
            {
                return true;
            }

            if (fromRole is not null)
            {
                var restored = await _userManager.AddToRoleAsync(user, fromRole);
                _logger.LogError(
                    "Could not add role {NewRole} to user {UserId}; restored {OldRole}: {Restored}", 
                    toRole, user.Id, fromRole, restored.Succeeded);
            }

            return false;
        }

        private async Task<EditUserViewModel> BuildEditModelAsync(
            ApplicationUser user, EditUserViewModel? posted = null)
        {
            var currentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty;
            var isProtected = await IsProtectedAsync(user);

            var model = posted ?? new EditUserViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = currentRole,
            };

            model.Id = user.Id;
            model.Email = user.Email ?? string.Empty;
            model.IsProtected = isProtected;
            model.IsActive = user.IsActive;
            model.InvitationPending = user.PasswordHash is null;

            if (isProtected)
            {
                model.Role = currentRole;
                model.AvailableRoles = [new SelectListItem(currentRole, currentRole)];
            }
            else
            {
                model.AvailableRoles = BuildRoleOptions();
            }

            return model;
        }
    }
}
