using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.User
{
    public class UserDetailDto
    {
        public string Id { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string[] Roles { get; init; } = [];
        public bool IsActive { get; init; }
        public bool EmailConfirmed { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? LastLoginDate { get; init; }
        public int FailedLoginAttempts { get; init; }
        public DateTime? lockoutEnd { get; init; }
    }
}
