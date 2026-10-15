using Microsoft.AspNetCore.Mvc;

namespace AutomotiveDMS.Web.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string AbsoluteActionFromAppDomain(
            this IUrlHelper url, IConfiguration configuration,
            string action, string controller, object routeValues)
        {
            var baseUrl = configuration["App:Domain"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("App:Domain is not configured");

            var path = url.Action(action, controller, routeValues)
                ?? throw new InvalidOperationException($"No route for {controller}/{action}.");

            return $"{baseUrl}{path}";
        }
    }
}
