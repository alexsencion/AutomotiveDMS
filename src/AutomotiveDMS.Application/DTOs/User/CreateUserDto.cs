using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.User
{
    public class CreateUserDto
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string TemporaryPassword { get; init; } = string.Empty;
        public string[] Roles { get; init; } = [];
    }
}
