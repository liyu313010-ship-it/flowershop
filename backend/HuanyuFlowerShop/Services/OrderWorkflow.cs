using System.Data;
using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HuanyuFlowerShop.Services;

/// <summary>用户、管理端和超时任务共用的订单状态操作。</summary>
public static class OrderWorkflow
{
    public static async Task<Order?> ChangeStatusAsync(ApplicationDbContext db, ICacheService cache,
        int orderId, string status, int? ownerId, string operatorName, string? note = null,
        string? company = null, string? trackingNumber = null, bool expiredOnly = false,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken) : null;
        Order? order;
        if (db.Database.IsRelational())
        {
            var rows = await db.Orders.FromSqlInterpolated($"SELECT * FROM `Orders` WHERE `Id` = {orderId} FOR UPDATE")
                .AsNoTracking().ToListAsync(cancellationToken);
            var snapshot = rows.SingleOrDefault();
            if (snapshot is null) return null;
            order = db.Orders.Local.FirstOrDefault(o => o.Id == orderId);
            if (order is null) { order = snapshot; db.Attach(order); }
            else db.Entry(order).CurrentValues.SetValues(snapshot);
        }
        else order = await db.Orders.FindAsync(new object[] { orderId }, cancellationToken);

        if (order is null || (ownerId.HasValue && order.UserId != ownerId)) return null;
        if (expiredOnly && (order.Status != "pending" || order.PaymentStatus != "unpaid" || order.PaymentMethod != "online" ||
            !order.PaymentExpiresAt.HasValue || order.PaymentExpiresAt > DateTime.UtcNow)) return null;

        status = status.Trim().ToLowerInvariant();
        var oldStatus = order.Status;
        var hasShipping = !string.IsNullOrWhiteSpace(company) && !string.IsNullOrWhiteSpace(trackingNumber);
        if (status == oldStatus && !(status == "shipped" && hasShipping)) return order;
        var allowed = oldStatus switch
        {
            "pending" => status is "processing" or "cancelled",
            "processing" => status is "shipped" or "cancelled",
            "shipped" => status == "delivered" || (status == "shipped" && hasShipping),
            _ => false
        };
        if (!allowed) throw new ArgumentException($"订单不能从{oldStatus}变更为{status}");
        if (status == "shipped" && !hasShipping) throw new ArgumentException("发货必须提供物流公司和运单号");
        if (company?.Length > 100 || trackingNumber?.Length > 100) throw new ArgumentException("物流信息过长");

        var now = DateTime.UtcNow;
        if (status == "cancelled")
        {
            var items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).OrderBy(i => i.ProductId).ToListAsync(cancellationToken);
            foreach (var item in items)
            {
                if (db.Database.IsRelational())
                {
                    await db.Products.Where(p => p.Id == item.ProductId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + item.Quantity), cancellationToken);
                    var tracked = db.Products.Local.FirstOrDefault(p => p.Id == item.ProductId);
                    if (tracked is not null) await db.Entry(tracked).ReloadAsync(cancellationToken);
                }
                else
                {
                    var product = await db.Products.FindAsync(new object[] { item.ProductId }, cancellationToken);
                    if (product is not null) product.Stock += item.Quantity;
                }
            }
            if (order.DiscountAmount > 0 && (order.CouponId.HasValue || !string.IsNullOrEmpty(order.CouponCode)))
            {
                var coupon = order.CouponId.HasValue
                    ? await db.Coupons.FindAsync(new object[] { order.CouponId.Value }, cancellationToken)
                    : await db.Coupons.FirstOrDefaultAsync(c => c.Code == order.CouponCode, cancellationToken);
                if (coupon is not null)
                {
                    var reservation = await db.UserCoupons.FirstOrDefaultAsync(u => u.UserId == order.UserId && u.CouponId == coupon.Id, cancellationToken);
                    if (reservation?.Status == "used")
                    {
                        reservation.Status = "claimed";
                        reservation.UsedAt = null;
                        if (db.Database.IsRelational())
                        {
                            await db.Coupons.Where(c => c.Id == coupon.Id && c.UsedCount > 0)
                                .ExecuteUpdateAsync(s => s.SetProperty(c => c.UsedCount, c => c.UsedCount - 1), cancellationToken);
                            await db.Entry(coupon).ReloadAsync(cancellationToken);
                        }
                        else coupon.UsedCount = Math.Max(0, coupon.UsedCount - 1);
                    }
                }
            }
        }
        order.Status = status;
        order.UpdatedAt = now;
        if (status == "shipped") order.ShippedAt = now;
        if (status == "delivered")
        {
            order.DeliveredAt = now;
            var earnedPoints = Math.Max(1, (int)Math.Floor(order.TotalAmount));
            if (db.Database.IsRelational())
                await db.Users.Where(u => u.Id == order.UserId).ExecuteUpdateAsync(s => s.SetProperty(u => u.Points, u => u.Points + earnedPoints), cancellationToken);
            else
            {
                var user = await db.Users.FindAsync(new object[] { order.UserId }, cancellationToken);
                if (user is not null) user.Points += earnedPoints;
            }
        }
        if (!string.IsNullOrWhiteSpace(note)) order.Message = note;
        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = orderId, OldStatus = oldStatus, NewStatus = status,
            OperatorId = ownerId, OperatorName = operatorName, CreatedAt = now,
            Note = status == "shipped" ? $"物流公司：{company!.Trim()}；运单号：{trackingNumber!.Trim()}" : note ?? $"订单状态从{oldStatus}更新为{status}"
        });
        await db.SaveChangesAsync(cancellationToken);
        if (transaction is not null) await transaction.CommitAsync(cancellationToken);
        await OrderCacheInvalidation.InvalidateAsync(cache, order.UserId, order.Id);
        return order;
    }
}
