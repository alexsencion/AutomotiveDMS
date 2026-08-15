using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool IsActive { get; init; } = true;
        public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }
    }
}
