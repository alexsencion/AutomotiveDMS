using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AutomotiveDMS.Web.ViewModels.Users
{
    public class EditUserViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [RegularExpression(@"^\+?[0-9\s\-()]{7,20}$", ErrorMessage = "Enter a valid phone number")]
        [StringLength(20)]
        [Display(Name = "Phone (optional)")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Select a role")]
        public string Role { get; set; } = string.Empty;



        [BindNever, ValidateNever]
        public string Id { get; set; } = string.Empty;

        [BindNever, ValidateNever]
        public string Email { get; set; } = string.Empty;

        [BindNever, ValidateNever]
        public bool IsProtected { get; set; }

        [BindNever, ValidateNever]
        public bool IsActive { get; set; }

        [BindNever, ValidateNever]
        public bool InvitationPending { get; set; }

        [BindNever, ValidateNever]
        public IEnumerable<SelectListItem> AvailableRoles { get; set; } = [];
    }
}
