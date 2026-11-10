using AutomotiveDMS.Application.Common.Models;
using AutomotiveDMS.Application.DTOs.Audit;
using AutomotiveDMS.Domain.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AutomotiveDMS.Web.ViewModels.Audit
{
    public class AuditIndexViewModel
    {
        public string? EntityName { get; set; }
        public string? EntityId { get; set; }
        public string? ChangedBy { get; set; }
        public AuditAction? ActionType { get; set; }
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
        public int Page { get; set; } = 1;

        [BindNever, ValidateNever] public PagedResult<AuditLogEntryDto> Result { get; set; } = PagedResult<AuditLogEntryDto>.Empty(25);
        [BindNever, ValidateNever] public IEnumerable<SelectListItem> EntityNames { get; set; } = [];
        [BindNever, ValidateNever] public IEnumerable<SelectListItem> Users { get; set; } = [];
        [BindNever, ValidateNever] public IEnumerable<SelectListItem> Actions { get; set; } = [];

        public Dictionary<string, string> RouteValuesForPage(int page)
        {
            var values = new Dictionary<string, string> { ["Page"] = page.ToString() };
            if (!string.IsNullOrWhiteSpace(EntityName)) values["EntityName"] = EntityName;
            if (!string.IsNullOrWhiteSpace(EntityId)) values["EntityId"] = EntityId;
            if (!string.IsNullOrWhiteSpace(ChangedBy)) values["ChangedBy"] = ChangedBy;
            if (ActionType is { } a) values["ActionType"] = a.ToString();
            if (From is { } f) values["From"] = f.ToString("yyyy-MM-dd");
            if (To is { } t) values["To"] = t.ToString("yyyy-MM-dd");

            return values;
        }
    }
}
