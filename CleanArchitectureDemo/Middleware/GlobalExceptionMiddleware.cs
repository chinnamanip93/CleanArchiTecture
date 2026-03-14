using System.Net;
using System.Text.Json;
using Domain.Exceptions;

namespace CleanArchitectureDemo.Middleware
{
    /// <summary>
    /// Catches all unhandled exceptions and converts them into
    /// consistent RFC 7807 Problem-Details JSON responses.
    ///
    /// Registered first in Program.cs:
    ///   app.UseMiddleware&lt;GlobalExceptionMiddleware&gt;();
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (statusCode, title, detail) = ex switch
            {
                NotFoundException nfe =>
                    (HttpStatusCode.NotFound, "Resource Not Found", nfe.Message),

                DomainException de =>
                    (HttpStatusCode.BadRequest, "Domain Error", de.Message),

                UnauthorizedAccessException =>
                    (HttpStatusCode.Unauthorized, "Unauthorized", "Authentication is required."),

                _ =>
                    (HttpStatusCode.InternalServerError, "Internal Server Error",
                     _env.IsDevelopment() ? ex.Message : "An unexpected error occurred.")
            };

            // Log unhandled 500s at Error level; domain/business errors at Warning
            if (statusCode == HttpStatusCode.InternalServerError)
                _logger.LogError(ex, "Unhandled exception [{Method}] {Path}", context.Request.Method, context.Request.Path);
            else
                _logger.LogWarning(ex, "Handled exception ({Status}) [{Method}] {Path}",
                    (int)statusCode, context.Request.Method, context.Request.Path);

            var problem = new
            {
                type = $"https://httpstatuses.io/{(int)statusCode}",
                title,
                status = (int)statusCode,
                detail,
                traceId = context.TraceIdentifier
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem, _jsonOptions));
        }
    }
}
