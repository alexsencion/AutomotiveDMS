using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AutomotiveDMS.Web.ViewModels.Users
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter a valid phone number")]
        [StringLength(20)]
        [Display(Name = "Phone (optional)")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Select a role")]
        public string Role { get; set; } = string.Empty;

        [BindNever, ValidateNever]
        public IEnumerable<SelectListItem> AvailableRoles { get; set; } = [];
    }
}
