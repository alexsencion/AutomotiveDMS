using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Enums
{
    public enum LoginEventType
    {
        Succeeded = 1,
        FailedPassword = 2,
        FailedUnknownUser = 3,
        LockoutTriggered = 4,
        LockoutOutAttempt = 5,
        BlockedInactive = 6,
        LoggedOut = 7
    }
}
