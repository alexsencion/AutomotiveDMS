using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.User
{
    public class UpdateUserDto
    {
        public string Id { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string[] Roles { get; init; } = [];
        public bool IsActive { get; init; }
    }
}
