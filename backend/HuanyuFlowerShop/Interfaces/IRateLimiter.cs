namespace HuanyuFlowerShop.Interfaces
{
    /// <summary>
    /// 速率限制器接口
    /// </summary>
    public interface IRateLimiter
    {
        /// <summary>
        /// 检查请求是否允许
        /// </summary>
        /// <param name="key">客户端标识键</param>
        /// <returns>是否允许请求</returns>
        bool TryAllowRequest(string key);
        
        /// <summary>
        /// 设置速率限制规则
        /// </summary>
        /// <param name="requestsPerMinute">每分钟允许的请求数</param>
        void SetLimit(int requestsPerMinute);
    }
}