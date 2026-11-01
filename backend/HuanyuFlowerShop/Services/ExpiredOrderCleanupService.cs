using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HuanyuFlowerShop.Services;

/// <summary>
/// 定期关闭未支付超时订单并释放已预扣库存。
/// 该任务使用数据库事务和状态条件，重复执行不会重复增加库存。
/// </summary>
public sealed class ExpiredOrderCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredOrderCleanupService> _logger;

    public ExpiredOrderCleanupService(IServiceScopeFactory scopeFactory, ILogger<ExpiredOrderCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CleanupAsync(stoppingToken);
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CleanupAsync(stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
            var now = DateTime.UtcNow;
            var orders = await db.Orders
                .AsNoTracking()
                .Where(o => o.Status == "pending" && o.PaymentStatus == "unpaid" && o.PaymentMethod == "online" &&
                            o.PaymentExpiresAt.HasValue && o.PaymentExpiresAt.Value <= now)
                .Take(100)
                .ToListAsync(cancellationToken);

            if (orders.Count == 0) return;

            foreach (var order in orders)
            {
                await OrderWorkflow.ChangeStatusAsync(db, cache, order.Id, "cancelled", null, "system",
                    "支付超时，系统自动关闭订单并释放库存及优惠券", expiredOnly: true, cancellationToken: cancellationToken);
            }
            _logger.LogInformation("自动关闭支付超时订单 {Count} 个", orders.Count);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "自动关闭超时订单失败");
        }
    }
}
