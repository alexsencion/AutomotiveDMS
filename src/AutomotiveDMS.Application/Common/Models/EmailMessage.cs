using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.Common.Models
{
    public sealed record EmailMessage(
        string To,
        string ToName,
        string Subject,
        string HtmlBody,
        string PlainTextBody);
}
