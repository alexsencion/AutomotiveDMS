using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? IpAddress { get; }
    }

    public static class CurrentUserDefaults
    {
        public const string System = "SYSTEM";
    }
}
