using AutomotiveDMS.Application.Interfaces.Services;
using System.Security.Claims;

namespace AutomotiveDMS.Web.Services
{
    public sealed class HttpContextCurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _accessor;

        public HttpContextCurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;
        public string? UserId =>
            _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        public string? IpAddress =>
            _accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }
}
