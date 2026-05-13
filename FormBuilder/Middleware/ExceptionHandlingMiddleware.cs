using FluentValidation;
using FormBuilder.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FormBuilder.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
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
            catch (AppException ex)
            {
                _logger.LogWarning(ex, "Application exception ({StatusCode}): {Message}", ex.StatusCode, ex.Message);
                await WriteAsync(context, ex.StatusCode, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Forbidden: {Message}", ex.Message);
                await WriteAsync(context, 403, ex.Message);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failed: {Errors}", string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
                var errors = ex.Errors
                    .Select(e => string.IsNullOrEmpty(e.PropertyName) ? e.ErrorMessage : $"{e.PropertyName}: {e.ErrorMessage}")
                    .Distinct()
                    .ToList();
                await WriteAsync(context, 422, "One or more validation errors occurred.", errors);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Invalid security token.");
                await WriteAsync(context, 401, "Invalid or expired token.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception.");
                var message = _env.IsDevelopment() ? ex.Message : "An unexpected error occurred. Please try again later.";
                await WriteAsync(context, 500, message);
            }
        }

        private static Task WriteAsync(HttpContext context, int statusCode, string message, List<string>? errors = null)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            var body = JsonSerializer.Serialize(new { message, errors }, JsonOptions);
            return context.Response.WriteAsync(body);
        }
    }
}
