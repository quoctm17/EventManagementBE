using System.Net;
using EventManagement.Application.DTOs.Responses;
using EventManagement.Application.Constants;

namespace EventManagement.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // chạy xuống pipeline tiếp theo
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred.");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = new HTTPResponseValue<string>(
                    null,
                    StatusResponse.Error,
                    MessageResponse.Error
                );

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }

    // Extension method để tiện Add vào Program.cs
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
    }
}