using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.Security
{
    public sealed record LoginAttemptOutcome(
        bool UserFound,
        bool Succeeded,
        bool IsLockedOut,
        bool IsNotAllowed,
        bool WasLockedOutBefore);
}
