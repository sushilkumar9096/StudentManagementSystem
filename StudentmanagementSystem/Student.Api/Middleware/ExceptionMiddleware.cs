using System.Net;
using System.Text.Json;
using Student.Core.Common;
using Student.Core.Exceptions;

namespace Student.Api.Middleware
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
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>();

            switch (exception)
            {
                case NotFoundException notFoundEx:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = ApiResponse<object>.FailureResponse(notFoundEx.Message, statusCode: 404);
                    break;

                case ValidationException validationEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.FailureResponse(validationEx.Message, validationEx.Errors, statusCode: 400);
                    break;

                case Core.Exceptions.UnauthorizedAccessException authEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = ApiResponse<object>.FailureResponse(authEx.Message, statusCode: 401);
                    break;

                case ConflictException conflictEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    response = ApiResponse<object>.FailureResponse(conflictEx.Message, statusCode: 409);
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response = ApiResponse<object>.FailureResponse(
                        "An unexpected server error occurred. Please try again later.",
                        new List<string> { exception.Message },
                        statusCode: 500
                    );
                    break;
            }

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, jsonOptions);

            return context.Response.WriteAsync(json);
        }
    }
}
