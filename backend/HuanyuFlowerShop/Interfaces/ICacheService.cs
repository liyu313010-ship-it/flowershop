namespace HuanyuFlowerShop.Interfaces;

/// <summary>
/// 缓存服务接口
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// 从缓存中获取数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <returns>缓存的数据，如果不存在则返回null</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// 将数据存入缓存
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">要缓存的数据</param>
    /// <param name="expiry">过期时间，如果为null则使用默认过期时间</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    /// <summary>
    /// 移除缓存项
    /// </summary>
    /// <param name="key">缓存键</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// 检查缓存项是否存在
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <returns>缓存项是否存在</returns>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// 根据模式移除缓存项
    /// </summary>
    /// <param name="pattern">缓存键模式</param>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// 清空所有缓存
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// 获取缓存项的过期时间
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <returns>过期时间，如果不存在则返回null</returns>
    Task<TimeSpan?> GetExpiryAsync(string key);
}