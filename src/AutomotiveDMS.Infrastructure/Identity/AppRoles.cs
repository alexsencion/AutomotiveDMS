using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Identity
{
    public class AppRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Secretary = "Secretary";

        public static readonly IReadOnlyList<string> All = [Admin, Manager, Secretary];

        public static readonly IReadOnlyList<string> AssignableOnCreation = [Manager, Secretary];
    }
}
