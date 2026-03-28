using System.Net;
using System.Text.Json;

namespace FormBuilder.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access.");
                await HandleExceptionAsync(context, ex, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");
                
                var status = ex is ArgumentException || ex is InvalidOperationException || ex.Message.Contains("not found") 
                    ? HttpStatusCode.BadRequest 
                    : HttpStatusCode.InternalServerError;

                await HandleExceptionAsync(context, ex, status);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            var result = JsonSerializer.Serialize(new { message = exception.Message });
            return context.Response.WriteAsync(result);
        }
    }
}
