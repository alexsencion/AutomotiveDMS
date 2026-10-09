using AutomotiveDMS.Application.Common.Models;
using System.Text.Encodings.Web;

namespace AutomotiveDMS.Web.Emails
{
    public static class PasswordResetEmail
    {
        public static EmailMessage Create(
            string recepientEmail, string recipientName, string resetLink, TimeSpan validFor)
        {
            var name = HtmlEncoder.Default.Encode(recepientEmail);
            var link = HtmlEncoder.Default.Encode(resetLink);
            var minutes = (int)validFor.TotalMinutes;

            var html = $"""
                <p>Hello {name},</p>
                <p>We received a request to reset your Automotive DMS password.</p>
                <p><a href="{link}">Reset your password</a></p>
                <p>This link expires in {minutes} minutes and can be used once.</p>
                <p>If you did not request this, you can ignore this email.
                   Your password will not change.</p>
                """;

            var text = $"""
                Hello {recipientName},

                We received a request to reset your Automotive DMS password.

                Reset your password: {resetLink}

                This link expires in {minutes} minutes and can be used once.
                If you did not request this, you can ignore this email.
                """;

            return new EmailMessage(
                To: recepientEmail,
                ToName: recipientName,
                Subject: "Reset your Automotive DMS password",
                HtmlBody: html,
                PlainTextBody: text);
        }
    }
}
