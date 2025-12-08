using System;
using Microsoft.Extensions.Logging;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;

namespace HuanyuFlowerShop.Services
{
    /// <summary>
    /// 审计日志服务实现
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly IRepository<AuditLog> _auditLogRepository;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(IRepository<AuditLog> auditLogRepository, ILogger<AuditLogService> logger)
        {
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        public async Task LogAsync(int? userId, string action, string resource, string? resourceId, string? details = null, string? ipAddress = null)
        {
            try
            {
                // 创建审计日志记录
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    Resource = resource,
                    ResourceId = resourceId,
                    Details = details,
                    CreatedAt = DateTime.UtcNow,
                    IPAddress = ipAddress ?? "127.0.0.1" // 使用传入的IP地址，如果为null则使用默认值
                };

                // 保存到数据库
                // CreateAsync方法内部已经包含SaveChangesAsync调用，不需要单独调用
                await _auditLogRepository.CreateAsync(auditLog);
            }
            catch (Exception ex)
            {
                // 记录失败时不影响主要业务流程，只记录错误日志
                _logger.LogWarning(ex, "记录审计日志失败: {Message}", ex.Message);
                // 记录简化的审计日志到控制台，用于调试
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    Console.WriteLine($"[审计日志] {DateTime.UtcNow}: {action} {resource} {resourceId} {userId}");
                }
            }
        }
    }
}