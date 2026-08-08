using AutomotiveDMS.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace AutomotiveDMS.Web.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex,
                    "Domain rule violation on {Method} {Path}: {Message}",
                    context.Request.Method,
                    context.Request.Path,
                    ex.Message);

                await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError);
            }
        }
        private async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception,
            HttpStatusCode statusCode)
        {
            context.Response.StatusCode = (int)statusCode;

            if (IsAjaxRequest(context.Request))
            {
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = _environment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occured. Please try again.",
                    statusCode = (int)statusCode
                };

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response));
            }
            else
            {
                context.Response.Redirect(
                    $"/Home/Error?statusCode={(int)statusCode}");
            }
        }

        private static bool IsAjaxRequest(HttpRequest request) =>
            request.Headers.XRequestedWith == "XMLHttpRequest" ||
            request.Headers.Accept.ToString().Contains("application/json");
    }
}
