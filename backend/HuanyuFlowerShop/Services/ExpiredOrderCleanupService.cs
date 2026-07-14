using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;
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
            var now = DateTime.UtcNow;
            var orders = await db.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.Status == "pending" && o.PaymentStatus == "unpaid" &&
                            o.PaymentExpiresAt.HasValue && o.PaymentExpiresAt.Value <= now)
                .Take(100)
                .ToListAsync(cancellationToken);

            if (orders.Count == 0) return;

            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            foreach (var order in orders)
            {
                // 重新确认状态，避免与用户/管理员取消操作竞争。
                var current = await db.Orders.FirstOrDefaultAsync(o => o.Id == order.Id, cancellationToken);
                if (current is null || current.Status != "pending" || current.PaymentStatus != "unpaid") continue;

                foreach (var item in order.OrderItems)
                {
                    var product = await db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);
                    if (product is not null) product.Stock += item.Quantity;
                }

                var oldStatus = current.Status;
                current.Status = "cancelled";
                current.UpdatedAt = now;
                db.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    OrderId = current.Id,
                    OldStatus = oldStatus,
                    NewStatus = "cancelled",
                    OperatorName = "system",
                    Note = "支付超时，系统自动关闭订单并释放库存",
                    CreatedAt = now
                });
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("自动关闭支付超时订单 {Count} 个", orders.Count);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "自动关闭超时订单失败");
        }
    }
}
