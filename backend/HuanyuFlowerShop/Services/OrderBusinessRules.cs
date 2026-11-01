using HuanyuFlowerShop.Entities;

namespace HuanyuFlowerShop.Services;

/// <summary>报价和下单共用的业务规则；优惠仅抵扣商品金额。</summary>
public static class OrderBusinessRules
{
    public static decimal ShippingFee(decimal subtotal, string method, IConfiguration configuration)
    {
        if (method is not ("standard" or "same_day")) throw new ArgumentException("不支持的配送方式");
        var fee = subtotal >= configuration.GetValue("Shipping:FreeThreshold", 299m)
            ? 0m : configuration.GetValue("Shipping:BaseFee", 15m);
        if (method == "same_day") fee += configuration.GetValue("Shipping:SameDayFee", 20m);
        return fee;
    }

    public static decimal CouponDiscount(Coupon? coupon, UserCoupon? userCoupon, decimal subtotal, DateTime now)
    {
        if (coupon is null || coupon.Status != "active") throw new ArgumentException("优惠券无效或未启用");
        if ((coupon.StartAt.HasValue && coupon.StartAt > now) || (coupon.EndAt.HasValue && coupon.EndAt < now))
            throw new ArgumentException("优惠券不在有效期");
        if (userCoupon?.Status == "used") throw new ArgumentException("该优惠券已经使用");
        if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
            throw new ArgumentException("优惠券已用完");
        if (subtotal < coupon.MinOrderAmount) throw new ArgumentException("未达到优惠券使用门槛");
        if (coupon.Value <= 0 || coupon.DiscountType is not ("amount" or "percent"))
            throw new ArgumentException("优惠券配置无效");
        var discount = coupon.DiscountType == "percent"
            ? Math.Round(subtotal * coupon.Value / 100m, 2) : coupon.Value;
        if (coupon.MaxDiscount.HasValue) discount = Math.Min(discount, coupon.MaxDiscount.Value);
        return Math.Max(0m, Math.Min(subtotal, discount));
    }
}
