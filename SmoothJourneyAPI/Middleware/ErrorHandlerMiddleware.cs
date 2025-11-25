using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SmoothJourneyAPI.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        private async Task HandleException(HttpContext ctx, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            var code = HttpStatusCode.InternalServerError;
            string message = "An unexpected error occurred.";

            switch (ex)
            {
                case ApplicationException:
                    code = HttpStatusCode.BadRequest;
                    message = ex.Message;
                    break;
                case UnauthorizedAccessException:
                    code = HttpStatusCode.Unauthorized;
                    message = "Unauthorized";
                    break;
                case KeyNotFoundException:
                    code = HttpStatusCode.NotFound;
                    message = ex.Message;
                    break;
            }

            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode = (int)code;

            var response = new
            {
                statusCode = ctx.Response.StatusCode,
                error = message,
                timestamp = DateTime.UtcNow
            };

            await ctx.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
