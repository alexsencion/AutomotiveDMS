using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.User
{
    public class UserListDto
    {
        public string Id { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string[] Roles { get; init; } = [];
        public bool IsActive { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? LastLoginDate { get; init; }
    }
}
