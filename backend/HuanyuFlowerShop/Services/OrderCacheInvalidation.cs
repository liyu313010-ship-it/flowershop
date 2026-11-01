using HuanyuFlowerShop.Interfaces;

namespace HuanyuFlowerShop.Services;

public static class OrderCacheInvalidation
{
    public static async Task InvalidateAsync(ICacheService cache, int userId, int? orderId = null)
    {
        await cache.RemoveAsync($"user_orders_{userId}");
        await cache.RemoveAsync($"user_order_stats_{userId}");
        await cache.RemoveAsync($"user_id_{userId}");
        if (orderId.HasValue)
        {
            await cache.RemoveAsync($"order_{orderId}_{userId}");
            await cache.RemoveAsync($"order_history_{orderId}");
            await cache.RemoveAsync($"order_history_{userId}_{orderId}");
        }
        await cache.RemoveAsync("all_products");
        await cache.RemoveAsync("featured_products");
        await cache.RemoveAsync("home_products");
        await cache.RemoveByPatternAsync("product_*");
        await cache.RemoveByPatternAsync("products_*");
    }
}
