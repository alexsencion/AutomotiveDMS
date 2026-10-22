using AutomotiveDMS.Application.Common.Models;
using System.Text.Encodings.Web;

namespace AutomotiveDMS.Web.Emails
{
    public static class InvitationEmail
    {
        public static EmailMessage Create(
            string recipientEmail, string recipientName, string role,
            string setPasswordLink, TimeSpan validFor)
        {
            var name = HtmlEncoder.Default.Encode(recipientEmail);
            var link = HtmlEncoder.Default.Encode(setPasswordLink);
            var r = HtmlEncoder.Default.Encode(role);
            var days = (int)validFor.TotalDays;

            var html = $"""
            <p>Hello {name},</p>
            <p>An Automotive DMS account has been created for you with the
               <strong>{r}</strong> role.</p>
            <p><a href="{link}">Set your password</a></p>
            <p>This link expires in {days} days and can be used once.
               If it expires, ask the administrator to resend it.</p>
            """;

            var text = $"""
            Hello {recipientName},

            An Automotive DMS account has been created for you with the {role} role.

            Set your password: {setPasswordLink}

            This link expires in {days} days and can be used once.
            If it expires, ask the administrator to resend it.
            """;

            return new EmailMessage(recipientEmail, recipientName,
                "Your Automotive DMS account", html, text);
        }
    }
}
