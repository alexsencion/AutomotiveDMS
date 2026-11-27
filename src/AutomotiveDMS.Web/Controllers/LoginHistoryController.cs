using AutomotiveDMS.Application.DTOs.LoginHistory;
using AutomotiveDMS.Application.Interfaces.Services;
using AutomotiveDMS.Domain.Enums;
using AutomotiveDMS.Infrastructure.Identity;
using AutomotiveDMS.Web.Extensions;
using AutomotiveDMS.Web.ViewModels.LoginHistory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutomotiveDMS.Web.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class LoginHistoryController : Controller
    {
        private const int PageSize = 25;

        private readonly ILoginHistoryService _loginHistory;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginHistoryController(ILoginHistoryService loginHistory, UserManager<ApplicationUser> userManager)
        {
            _loginHistory = loginHistory;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(LoginHistoryIndexViewModel model, CancellationToken cancellationToken)
        {
            if (model.From is { } f && model.To is { } t && f > t)
            {
                ModelState.AddModelError(nameof(model.To), "'To' must be on or after 'From'.");
            }

            if (ModelState.IsValid)
            {
                model.Result = await _loginHistory.SearchAsync(new LoginHistoryQuery(
                    UserId: model.UserId,
                    Email: model.Email,
                    EventType: model.EventTypeFilter,
                    FromUtc: model.From is { } from ? DominicanTime.ToUtc(from.ToDateTime(TimeOnly.MinValue)) : null,
                    ToUtcExclusive: model.To is { } to ? DominicanTime.ToUtc(to.AddDays(1).ToDateTime(TimeOnly.MinValue)) : null,
                    Page: model.Page,
                    PageSize: PageSize), cancellationToken);
            }

            model.Users = (await _userManager.Users.AsNoTracking()
                    .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
                    .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName })
                    .ToListAsync(cancellationToken))
                .Select(u => new SelectListItem(u.Name, u.Id));

            model.EventTypes = Enum.GetValues<LoginEventType>()
                .Select(e => new SelectListItem(LoginEventDisplay.Label(e), e.ToString()));

            return View(model);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
