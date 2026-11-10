using AutomotiveDMS.Application.DTOs.Audit;
using AutomotiveDMS.Application.Interfaces.Services;
using AutomotiveDMS.Domain.Enums;
using AutomotiveDMS.Infrastructure.Identity;
using AutomotiveDMS.Web.Extensions;
using AutomotiveDMS.Web.ViewModels.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutomotiveDMS.Web.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AuditController : Controller
    {
        private const int PageSize = 25;

        private readonly IAuditService _audit;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditController(IAuditService audit, UserManager<ApplicationUser> userManager)
        {
            _audit = audit;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(AuditIndexViewModel model, CancellationToken cancellationToken)
        {
            if (model.From is { } f && model.To is { } t && f > t)
            {
                ModelState.AddModelError(nameof(model.To), "To' must be on or after 'From'.");
            }

            if (ModelState.IsValid)
            {
                var query = new AuditLogQuery(
                    EntityName: model.EntityName,
                    EntityId: model.EntityId,
                    ChangedBy: model.ChangedBy,
                    Action: model.ActionType,
                    FromUtc: model.From is { } from ? DominicanTime.ToUtc(from.ToDateTime(TimeOnly.MinValue)) : null,
                    ToUtcExclusive: model.To is { } to ? DominicanTime.ToUtc(to.AddDays(1).ToDateTime(TimeOnly.MinValue)) : null,
                    Page: model.Page,
                    PageSize: PageSize);

                model.Result = await _audit.SearchAsync(query, cancellationToken);
            }

            await PopulateOptionsAsync(model, cancellationToken);
            return View(model);
        }

        private async Task PopulateOptionsAsync(AuditIndexViewModel model, CancellationToken cancellationToken)
        {
            model.EntityNames = (await _audit.GetEntityNamesAsync(cancellationToken))
                .Select(n => new SelectListItem(n, n));

            var users = await _userManager.Users.AsNoTracking()
                .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
                .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName })
                .ToListAsync(cancellationToken);

            model.Users = users
                .Select(u => new SelectListItem(u.Name, u.Id))
                .Prepend(new SelectListItem("SYSTEM", "SYSTEM"));

            model.Actions = Enum.GetValues<AuditAction>()
                .Select(a => new SelectListItem(a.ToString(), a.ToString()));
        }
    }
}
