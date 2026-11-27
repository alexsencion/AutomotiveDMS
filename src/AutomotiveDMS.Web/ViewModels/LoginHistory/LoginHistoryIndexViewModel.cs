using AutomotiveDMS.Application.Common.Models;
using AutomotiveDMS.Application.DTOs.LoginHistory;
using AutomotiveDMS.Domain.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AutomotiveDMS.Web.ViewModels.LoginHistory
{
    public class LoginHistoryIndexViewModel
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public LoginEventType? EventTypeFilter { get; set; }
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
        public int Page { get; set; } = 1;

        [BindNever, ValidateNever] public PagedResult<LoginEventDto> Result { get; set; } = PagedResult<LoginEventDto>.Empty(25);
        [BindNever, ValidateNever] public IEnumerable<SelectListItem> Users { get; set; } = [];
        [BindNever, ValidateNever] public IEnumerable<SelectListItem> EventTypes { get; set; } = [];

        public Dictionary<string, string> RouteValuesForPage(int page)
        {
            var values = new Dictionary<string, string> { ["Page"] = page.ToString() };
            if (!string.IsNullOrWhiteSpace(UserId)) values["UserId"] = UserId;
            if (!string.IsNullOrWhiteSpace(Email)) values["Email"] = Email;
            if (EventTypeFilter is { } t) values["EventTypeFilter"] = t.ToString();
            if (From is { } f) values["From"] = f.ToString("yyyy-MM-dd");
            if (To is { } to) values["To"] = to.ToString("yyyy-MM-dd");
            return values;
        }
    }
}
