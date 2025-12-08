namespace HuanyuFlowerShop.Interfaces
{
    /// <summary>
    /// 审计日志服务接口
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>
        /// 记录审计日志
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="action">操作类型</param>
        /// <param name="resource">资源类型</param>
        /// <param name="resourceId">资源ID</param>
        /// <param name="details">详细信息</param>
        /// <param name="ipAddress">IP地址</param>
        Task LogAsync(int? userId, string action, string resource, string? resourceId, string? details = null, string? ipAddress = null);
    }
}