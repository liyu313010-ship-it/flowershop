using HuanyuFlowerShop.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HuanyuFlowerShop.Services;

/// <summary>
/// 内存缓存服务实现
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(30);

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 从缓存中获取数据
    /// </summary>
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                _logger.LogDebug("从缓存获取数据成功: {Key}", key);
                return value;
            }

            _logger.LogDebug("缓存中未找到数据: {Key}", key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从缓存获取数据失败: {Key}", key);
            return default;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 将数据存入缓存
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? _defaultExpiry,
                SlidingExpiration = TimeSpan.FromMinutes(5), // 滑动过期时间
                Priority = CacheItemPriority.Normal,
                Size = 1 // 设置缓存项大小
            };

            // 注册缓存项移除时的回调
            options.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
            {
                EvictionCallback = OnEviction,
                State = key
            });

            _cache.Set(key, value, options);
            _logger.LogDebug("数据存入缓存成功: {Key}, 过期时间: {Expiry}分钟", 
                           key, expiry?.TotalMinutes ?? _defaultExpiry.TotalMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据存入缓存失败: {Key}", key);
            throw;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 移除缓存项
    /// </summary>
    public async Task RemoveAsync(string key)
    {
        try
        {
            _cache.Remove(key);
            _logger.LogDebug("缓存项移除成功: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移除缓存项失败: {Key}", key);
            throw;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 检查缓存项是否存在
    /// </summary>
    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var exists = _cache.TryGetValue(key, out _);
            _logger.LogDebug("缓存项存在性检查: {Key}, 存在: {Exists}", key, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查缓存项存在性失败: {Key}", key);
            return false;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 根据模式移除缓存项
    /// </summary>
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            // 由于MemoryCache不支持模式匹配，这里实现为清空所有缓存
            // 在生产环境中可以考虑使用Redis等支持模式匹配的缓存
            if (pattern == "*")
            {
                await ClearAsync();
            }
            else
            {
                _logger.LogWarning("MemoryCache不支持模式匹配，仅支持通配符'*'清空所有缓存: {Pattern}", pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据模式移除缓存项失败: {Pattern}", pattern);
            throw;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 清空所有缓存
    /// </summary>
    public async Task ClearAsync()
    {
        try
        {
            if (_cache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0); // 压缩缓存，移除所有项
                _logger.LogInformation("所有缓存已清空");
            }
            else
            {
                _logger.LogWarning("当前缓存实现不支持清空操作");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清空缓存失败");
            throw;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 获取缓存项的过期时间
    /// </summary>
    public async Task<TimeSpan?> GetExpiryAsync(string key)
    {
        try
        {
            // MemoryCache不直接支持获取过期时间，这里返回null
            // 在实际应用中可以通过维护一个字典来跟踪过期时间
            _logger.LogDebug("MemoryCache不支持直接获取过期时间: {Key}", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取缓存项过期时间失败: {Key}", key);
            return null;
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 缓存项被移除时的回调
    /// </summary>
    private void OnEviction(object? key, object? value, EvictionReason reason, object? state)
    {
        _logger.LogDebug("缓存项被移除: {Key}, 原因: {Reason}", key ?? "[null]", reason);
    }
}