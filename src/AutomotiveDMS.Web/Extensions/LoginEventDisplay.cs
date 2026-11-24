using AutomotiveDMS.Domain.Enums;

namespace AutomotiveDMS.Web.Extensions
{
    public static class LoginEventDisplay
    {
        public static string Label(LoginEventType type) => type switch
        {
            LoginEventType.Succeeded => "Signed in",
            LoginEventType.FailedPassword => "Wrong password",
            LoginEventType.FailedUnknownUser => "Unknown account",
            LoginEventType.LockoutTriggered => "Lockout triggered",
            LoginEventType.LockoutOutAttempt => "Attempt while locked",
            LoginEventType.BlockedInactive => "Deactivated account",
            LoginEventType.LoggedOut => "Signed out",
            _                        => type.ToString()
        };

        public static string Badge(LoginEventType type) => type switch
        {
            LoginEventType.Succeeded => "text-bg-success",
            LoginEventType.LoggedOut => "text-bg-light border",
            LoginEventType.FailedPassword => "text-bg-warning",
            LoginEventType.FailedUnknownUser => "text-bg-secondary",
            LoginEventType.LockoutTriggered => "text-bg-danger",
            LoginEventType.LockoutOutAttempt => "text-bg-danger",
            LoginEventType.BlockedInactive => "text-bg-dark",
            _                              => "text-bg-secondary"
        };
    }
}
