using System.Net;
using System.Text.Json;
using HuanyuFlowerShop.Interfaces;
using Microsoft.Extensions.Configuration;

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

        public RateLimitingMiddleware(RequestDelegate next, IRateLimiter rateLimiter, IConfiguration configuration)
        {
            _next = next;
            _rateLimiter = rateLimiter;
            
            // 从配置文件读取速率限制设置
            int? rateLimit = configuration.GetSection("SecuritySettings:RateLimitPerMinute").Get<int>();
            _rateLimitPerMinute = rateLimit ?? 60;
            _rateLimiter.SetLimit(_rateLimitPerMinute);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 跳过静态文件和健康检查路由
            var path = context.Request.Path.Value;
            if (path != null && (path.StartsWith("/uploads") || path.StartsWith("/swagger") || path.StartsWith("/health")))
            {
                await _next(context);
                return;
            }

            // 获取客户端标识 - 优先使用用户ID（已认证），否则使用IP地址
            string clientKey;
            if (context.User?.Identity?.IsAuthenticated == true && context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) != null)
            {
                var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                clientKey = "user_" + (userIdClaim?.Value ?? "unknown");
            }
            else
            {
                clientKey = "ip_" + context.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";
            }

            // 检查是否允许请求
            if (!_rateLimiter.TryAllowRequest(clientKey))
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";
                
                var response = new {
                    Message = "请求过于频繁，请稍后再试",
                    RetryAfter = 60, // 建议60秒后重试
                    Limit = _rateLimitPerMinute
                };
                
                // 添加限流头部
                context.Response.Headers.Append("X-RateLimit-Limit", _rateLimitPerMinute.ToString());
                context.Response.Headers.Append("X-RateLimit-Retry-After", "60");
                
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }

            await _next(context);
        }
    }
}
