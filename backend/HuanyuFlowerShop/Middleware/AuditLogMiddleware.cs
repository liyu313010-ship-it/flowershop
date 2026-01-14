using System.Text;
using HuanyuFlowerShop.Interfaces;
using System.Security.Claims;

namespace HuanyuFlowerShop.Middleware
{
    /// <summary>
    /// 审计日志中间件
    /// </summary>
    public class AuditLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLogMiddleware> _logger;

        // 需要记录的敏感操作
        private readonly List<string> _sensitiveMethods = ["POST", "PUT", "DELETE", "PATCH"];
        
        // 需要跳过记录的路径
        private readonly List<string> _skipPaths =
        [
            "/swagger", "/health", "/favicon.ico", "/uploads"
        ];

        public AuditLogMiddleware(RequestDelegate next, ILogger<AuditLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            
            // 跳过不需要记录的路径
            if (_skipPaths.Any(path.StartsWith)) {
                await _next(context);
                return;
            }

            // 记录请求体（仅对敏感操作）
            string? requestBody = null;
            if (_sensitiveMethods.Contains(context.Request.Method))
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true);
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            // 保存原始响应流
            var originalResponseBody = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                // 执行后续中间件
                await _next(context);
            }
            finally
            {
                // 记录审计日志（针对敏感操作）
                if (_sensitiveMethods.Contains(context.Request.Method))
                {
                    await LogAuditEntry(context, path, requestBody, auditLogService);
                }

                // 复制响应到原始流
                responseBody.Position = 0;
                await responseBody.CopyToAsync(originalResponseBody);
                context.Response.Body = originalResponseBody;
            }
        }

        private async Task LogAuditEntry(HttpContext context, string path, string? requestBody, IAuditLogService auditLogService)
        {
            try
            {
                // 获取用户ID
                int? userId = null;
                if (context.User != null && context.User.Identity != null && context.User.Identity.IsAuthenticated)
                {
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier) ?? context.User.FindFirst("userId");
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id))
                    {
                        userId = id;
                    }
                }

                // 提取资源信息
                var (resource, resourceId) = ExtractResourceInfo(path);
                
                // 构建详细信息
                var details = new StringBuilder();
                details.AppendLine($"Method: {context.Request.Method}");
                details.AppendLine($"Path: {path}");
                details.AppendLine($"IP: {GetClientIpAddress(context)}");
                details.AppendLine($"UserAgent: {context.Request.Headers.UserAgent.ToString() ?? "unknown"}");
                if (!string.IsNullOrEmpty(requestBody) && requestBody.Length < 500) // 限制日志大小
                {
                    details.AppendLine($"RequestBody: {requestBody}");
                }

                // 获取客户端IP地址
                var clientIpAddress = GetClientIpAddress(context);
                
                // 记录审计日志
                await auditLogService.LogAsync(
                    userId: userId,
                    action: MapActionType(context.Request.Method),
                    resource: resource ?? "unknown",
                    resourceId: resourceId,
                    details: details.ToString().Trim(),
                    ipAddress: clientIpAddress
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录审计日志失败");
            }
        }

        private static (string resource, string? resourceId) ExtractResourceInfo(string path)
        {
            // 解析路径，提取资源类型和ID
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0)
            {
                var resource = segments[0].ToLower();
                string? resourceId = null;
                
                // 尝试提取资源ID（通常是第二个段）
                if (segments.Length > 1 && int.TryParse(segments[1], out _))
                {
                    resourceId = segments[1];
                }
                
                return (resource, resourceId);
            }
            return ("unknown", null);
        }

        private static string MapActionType(string method)
        {
            return method switch
            {
                "POST" => "create",
                "PUT" => "update",
                "DELETE" => "delete",
                "PATCH" => "partial_update",
                _ => "unknown"
            };
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            // 尝试从各种HTTP头获取真实IP
            var headers = context.Request.Headers;
            
            if (headers.TryGetValue("X-Forwarded-For", out var forwarded))
            {
                var forwardedIps = forwarded.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (forwardedIps.Length > 0)
                    return forwardedIps[0].Trim();
            }
            
            if (headers.TryGetValue("X-Real-IP", out var realIp))
            {
                return realIp.ToString().Trim();
            }
            
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
