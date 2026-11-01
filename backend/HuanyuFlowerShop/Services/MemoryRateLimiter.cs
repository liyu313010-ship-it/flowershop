using System.Collections.Concurrent;
using HuanyuFlowerShop.Interfaces;

namespace HuanyuFlowerShop.Services
{
    /// <summary>
    /// 基于内存的速率限制器实现
    /// </summary>
    public class MemoryRateLimiter : IRateLimiter
    {
        private readonly ConcurrentDictionary<string, Queue<DateTime>> _requestTimestamps;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _clientLocks;
        private int _requestsPerMinute;
        private readonly TimeSpan _timeWindow;

        public MemoryRateLimiter()
        {
            _requestTimestamps = new ConcurrentDictionary<string, Queue<DateTime>>();
            _clientLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
            _requestsPerMinute = 60; // 默认每分钟60个请求
            _timeWindow = TimeSpan.FromMinutes(1);
        }

        public bool TryAllowRequest(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            // 获取或创建客户端锁
            var clientLock = _clientLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            
            clientLock.Wait();
            try
            {
                var now = DateTime.UtcNow;
                
                // 获取或创建该客户端的请求时间队列
                var requestQueue = _requestTimestamps.GetOrAdd(key, _ => new Queue<DateTime>());
                
                // 移除过期的请求记录
                while (requestQueue.Count > 0 && (now - requestQueue.Peek()) > _timeWindow)
                {
                    requestQueue.Dequeue();
                }
                
                // 检查是否超过限制
                if (requestQueue.Count >= _requestsPerMinute)
                {
                    return false;
                }
                
                // 添加当前请求时间
                requestQueue.Enqueue(now);
                return true;
            }
            finally
            {
                clientLock.Release();
            }
        }

        public void SetLimit(int requestsPerMinute)
        {
            if (requestsPerMinute > 0)
            {
                _requestsPerMinute = requestsPerMinute;
            }
        }
    }
}