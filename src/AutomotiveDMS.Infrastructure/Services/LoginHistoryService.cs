using AutomotiveDMS.Application.Common.Models;
using AutomotiveDMS.Application.DTOs.LoginHistory;
using AutomotiveDMS.Application.Interfaces.Services;
using AutomotiveDMS.Domain.Entities;
using AutomotiveDMS.Domain.Enums;
using AutomotiveDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Services
{
    public class LoginHistoryService : ILoginHistoryService
    {
        private const int MaxPageSize = 100;

        private readonly ApplicationDbContext _db;
        public readonly ICurrentUserService _currentUser;
        public readonly ILogger<LoginHistoryService> _logger;

        public LoginHistoryService(
            ApplicationDbContext db, ICurrentUserService currentUser, ILogger<LoginHistoryService> logger)
        {
            _db = db;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task RecordAsync(
            LoginEventType eventType, string email, string? userId, CancellationToken cancellationToken = default)
        {
            var loginEvent = new LoginEvent
            {
                UserId = userId,
                Email = Truncate(email.Trim(), 256)!,
                EventType = eventType,
                OccurredAt = DateTime.UtcNow,
                IpAddress = _currentUser.IpAddress,
                UserAgent = Truncate(_currentUser.UserAgent, 512),
            };

            try
            {
                _db.LoginEvents.Add(loginEvent);
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _db.Entry(loginEvent).State = EntityState.Detached;
                _logger.LogError(ex, "Failed to record login event {EventType} for {Email}", eventType, email);
            }
        }

        public async Task<PagedResult<LoginEventDto>> SearchAsync(LoginHistoryQuery query, CancellationToken cancellationToken = default)
        {
            var page = Math.Max(1, query.Page);
            var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

            var events = _db.LoginEvents.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.UserId)) events = events.Where(e => e.UserId == query.UserId);
            if (!string.IsNullOrWhiteSpace(query.Email)) events = events.Where(e => e.Email.Contains(query.Email.Trim()));
            if (query.EventType is { } type) events = events.Where(e => e.EventType == type);
            if (query.FromUtc is { } from) events = events.Where(e => e.OccurredAt >= from);
            if (query.ToUtcExclusive is { } to) events = events.Where(e => e.OccurredAt < to);

            var total = await events.CountAsync(cancellationToken);

            var items = await (
                    from e in events
                    join u in _db.Users on e.UserId equals u.Id into users
                    from u in users.DefaultIfEmpty()
                    orderby e.OccurredAt descending, e.Id descending
                    select new LoginEventDto(
                        e.Id,
                        e.OccurredAt,
                        e.UserId,
                        u != null ? u.FirstName + " " + u.LastName : null,
                        e.Email,
                        e.EventType,
                        e.IpAddress,
                        e.UserAgent))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<LoginEventDto>(items, page, pageSize, total);
        }

        private static string? Truncate(string? value, int max) =>
            value is null || value.Length <= max ? value : value[..max];
    }
}