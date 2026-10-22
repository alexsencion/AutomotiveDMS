using AutomotiveDMS.Application.Common.Models;
using AutomotiveDMS.Application.Interfaces.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.IntegrationTests.Fixtures
{
    public sealed class FakeEmailService : IEmailService
    {
        private readonly ConcurrentQueue<EmailMessage> _sent = new();
        public IReadOnlyList<EmailMessage> Sent => _sent.ToList();

        public IReadOnlyList<EmailMessage> SentTo(string email) =>
            _sent.Where(m => string.Equals(m.To, email, StringComparison.OrdinalIgnoreCase))
                 .ToList();

        public Task<bool> SendAsync(EmailMessage message, CancellationToken ct = default)
        {
            _sent.Enqueue(message);
            return Task.FromResult(true);
        }
    }
}
