using HuanyuFlowerShop.Exceptions;
using System.Net;
using System.Text.Json;

namespace HuanyuFlowerShop.Middleware
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
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
                _logger.LogError(ex, "发生未处理的异常: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            var errorResponse = new ErrorResponse
            {
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                Method = context.Request.Method
            };

            switch (exception)
            {
                case ProductNotFoundException productNotFoundEx:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Code = "PRODUCT_NOT_FOUND";
                    errorResponse.Message = productNotFoundEx.Message;
                    errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case CategoryNotFoundException categoryNotFoundEx:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Code = "CATEGORY_NOT_FOUND";
                    errorResponse.Message = categoryNotFoundEx.Message;
                    errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case HuanyuFlowerShop.Exceptions.InvalidOperationException invalidOpEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Code = "INVALID_OPERATION";
                    errorResponse.Message = invalidOpEx.Message;
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case ArgumentNullException nullEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Code = "NULL_ARGUMENT";
                    errorResponse.Message = nullEx.Message;
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case ArgumentException argumentEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Code = "INVALID_ARGUMENT";
                    errorResponse.Message = argumentEx.Message;
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case UnauthorizedAccessException unauthorizedEx:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.Code = "UNAUTHORIZED";
                    errorResponse.Message = "未授权访问";
                    errorResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case BusinessException businessEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Code = "BUSINESS_ERROR";
                    errorResponse.Message = businessEx.Message;
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Code = "INTERNAL_SERVER_ERROR";
                    errorResponse.Message = "系统内部错误";
                    errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await response.WriteAsync(result);
        }
    }

    public class ErrorResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; }
        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
    }
}