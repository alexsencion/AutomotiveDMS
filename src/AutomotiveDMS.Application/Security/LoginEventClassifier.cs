using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.Security
{
    public static class LoginEventClassifier
    {
        public static LoginEventType Classify(LoginAttemptOutcome outcome)
        {
            ArgumentNullException.ThrowIfNull(outcome);
            EnsureConsistent(outcome);

            if (outcome.Succeeded) return LoginEventType.Succeeded;
            if (!outcome.UserFound) return LoginEventType.FailedUnknownUser;
            if (outcome.IsNotAllowed) return LoginEventType.BlockedInactive;
            if (outcome.IsLockedOut) return outcome.WasLockedOutBefore
                                            ? LoginEventType.LockoutOutAttempt
                                            : LoginEventType.LockoutTriggered;

            return LoginEventType.FailedPassword;
        }

        private static void EnsureConsistent(LoginAttemptOutcome o)
        {
            var resultFlags = (o.Succeeded ? 1 : 0) + (o.IsLockedOut ? 1 : 0) + (o.IsNotAllowed ? 1 : 0);

            if (resultFlags > 1)
                throw new ArgumentException(
                    "Succeeded, IsLockedOut and IsNotAllowed are mutually exclusive.", nameof(o));

            if (!o.UserFound && resultFlags > 0)
                throw new ArgumentException(
                    "An unknown user can only produce a plain failure.", nameof(o));
        }
    }
}
