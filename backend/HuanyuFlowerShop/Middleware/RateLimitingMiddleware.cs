using System.Net;
using System.Text.Json;
using HuanyuFlowerShop.Interfaces;

namespace HuanyuFlowerShop.Middleware
{
    /// <summary>
    /// 速率限制中间件
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRateLimiter _rateLimiter;
        private readonly int _rateLimitPerMinute;
        private readonly int _readRateLimitPerMinute;
        private readonly int _authRateLimitPerMinute;
        private readonly int _adminRateLimitPerMinute;

        public RateLimitingMiddleware(RequestDelegate next, IRateLimiter rateLimiter, IConfiguration configuration)
        {
            _next = next;
            _rateLimiter = rateLimiter;

            int? rateLimit = configuration.GetSection("SecuritySettings:RateLimitPerMinute").Get<int>();
            _rateLimitPerMinute = rateLimit ?? 60;
            _readRateLimitPerMinute = Math.Max(_rateLimitPerMinute * 6, 300);
            _authRateLimitPerMinute = Math.Max(_rateLimitPerMinute / 2, 30);
            _adminRateLimitPerMinute = Math.Max(_rateLimitPerMinute * 4, 240);
            _rateLimiter.SetLimit(_rateLimitPerMinute);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;
            if (ShouldSkipRateLimit(context, path))
            {
                await _next(context);
                return;
            }

            string clientKey;
            if (context.User?.Identity?.IsAuthenticated == true
                && context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) != null)
            {
                var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                clientKey = "user_" + (userIdClaim?.Value ?? "unknown");
            }
            else
            {
                clientKey = "ip_" + (context.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip");
            }

            var policy = GetPolicy(context, path);
            clientKey = $"{policy.KeyPrefix}_{clientKey}";

            if (!_rateLimiter.TryAllowRequest(clientKey, policy.LimitPerMinute))
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    Message = "请求过于频繁，请稍后再试",
                    RetryAfter = 60,
                    Limit = policy.LimitPerMinute
                };

                context.Response.Headers.Append("X-RateLimit-Limit", policy.LimitPerMinute.ToString());
                context.Response.Headers.Append("X-RateLimit-Retry-After", "60");

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }

            await _next(context);
        }

        private static bool IsRoute(HttpContext context, string path, string method, string route)
        {
            return string.Equals(context.Request.Method, method, StringComparison.OrdinalIgnoreCase)
                && path.Equals(route, StringComparison.OrdinalIgnoreCase);
        }

        private bool ShouldSkipRateLimit(HttpContext context, string? path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            return path.StartsWith("/assets", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/images", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/uploads", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/hubs", StringComparison.OrdinalIgnoreCase)
                || IsRoute(context, path, "POST", "/api/auth/logout");
        }

        private (string KeyPrefix, int LimitPerMinute) GetPolicy(HttpContext context, string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return ("default", _rateLimitPerMinute);
            }

            if (path.StartsWith("/api/admin", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/api/chat", StringComparison.OrdinalIgnoreCase))
            {
                return ("admin", _adminRateLimitPerMinute);
            }

            if (IsRoute(context, path, "POST", "/api/auth/login")
                || IsRoute(context, path, "POST", "/api/auth/register")
                || IsRoute(context, path, "POST", "/api/auth/forgot-password")
                || IsRoute(context, path, "POST", "/api/auth/reset-password"))
            {
                return ("auth", _authRateLimitPerMinute);
            }

            if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
            {
                return ("read", _readRateLimitPerMinute);
            }

            return ("default", _rateLimitPerMinute);
        }
    }
}
