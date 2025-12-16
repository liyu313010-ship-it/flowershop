using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HuanyuFlowerShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CouponsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("available")]
        public IActionResult GetAvailable([FromQuery] decimal? minAmount)
        {
            var now = DateTime.UtcNow;
            var list = _db.Coupons.Where(c => c.Status == "active"
                && (!c.StartAt.HasValue || c.StartAt <= now)
                && (!c.EndAt.HasValue || c.EndAt >= now)
                && (!minAmount.HasValue || c.MinOrderAmount <= minAmount.Value))
                .OrderByDescending(c => c.Value)
                .Select(c => new { c.Id, c.Code, c.DiscountType, c.Value, c.MinOrderAmount, c.MaxDiscount, c.UsageLimit, c.UsedCount, c.StartAt, c.EndAt })
                .ToList();
            return Ok(list);
        }

        [HttpPost("claim/{couponId}")]
        [Authorize]
        public IActionResult Claim(int couponId)
        {
            var uidStr = User.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
            if (!int.TryParse(uidStr, out var userId)) return Unauthorized();
            var coupon = _db.Coupons.FirstOrDefault(c => c.Id == couponId);
            if (coupon == null || coupon.Status != "active") return NotFound(new { message = "优惠券不存在或未启用" });
            var now = DateTime.UtcNow;
            if ((coupon.StartAt.HasValue && coupon.StartAt > now) || (coupon.EndAt.HasValue && coupon.EndAt < now))
                return BadRequest(new { message = "优惠券不在有效期" });
            var userClaimCount = _db.UserCoupons.Count(x => x.UserId == userId && x.CouponId == couponId);
            if (coupon.UsageLimitPerUser.HasValue && userClaimCount >= coupon.UsageLimitPerUser.Value)
                return BadRequest(new { message = "已达个人领取上限" });
            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                return BadRequest(new { message = "已达总发放上限" });
            var uc = new UserCoupon { UserId = userId, CouponId = couponId, ClaimedAt = now, Status = "claimed" };
            _db.UserCoupons.Add(uc);
            _db.SaveChanges();
            return Ok(new { message = "领取成功", id = uc.Id });
        }

        [HttpPost("validate")]
        public IActionResult Validate([FromBody] CouponValidateRequest req)
        {
            var coupon = _db.Coupons.FirstOrDefault(c => c.Code == req.Code && c.Status == "active");
            if (coupon == null) return NotFound(new { message = "优惠券无效" });
            
            // 检查用户是否已使用
            var uidStr = User.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
            if (int.TryParse(uidStr, out var userId))
            {
                var used = _db.UserCoupons.Any(uc => uc.UserId == userId && uc.CouponId == coupon.Id && uc.Status == "used");
                if (used) return BadRequest(new { message = "该优惠券您已使用过" });
            }

            var now = DateTime.UtcNow;
            if ((coupon.StartAt.HasValue && coupon.StartAt > now) || (coupon.EndAt.HasValue && coupon.EndAt < now))
                return BadRequest(new { message = "优惠券不在有效期" });
            if (req.OrderAmount < coupon.MinOrderAmount) return BadRequest(new { message = "未达到使用门槛" });
            decimal discount = 0;
            if ((coupon.DiscountType ?? "amount").ToLower() == "percent")
            {
                discount = Math.Round(req.OrderAmount * (coupon.Value / 100m), 2);
                if (coupon.MaxDiscount.HasValue) discount = Math.Min(discount, coupon.MaxDiscount.Value);
            }
            else
            {
                discount = coupon.Value;
            }
            return Ok(new { discount });
        }

        // 管理端 CRUD
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult List([FromQuery] int page = 1, [FromQuery] int limit = 20)
        {
            var q = _db.Coupons.OrderByDescending(c => c.CreatedAt);
            var total = q.Count();
            var data = q.Skip((page - 1) * limit).Take(limit).ToList();
            return Ok(new { total, data });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Create([FromBody] Coupon coupon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(coupon.Code)) return BadRequest(new { success = false, message = "Code必填" });
                if (_db.Coupons.Any(c => c.Code == coupon.Code)) return BadRequest(new { success = false, message = "Code重复" });
                
                // 确保必填字段都有值
                if (string.IsNullOrWhiteSpace(coupon.DiscountType)) coupon.DiscountType = "amount";
                if (string.IsNullOrWhiteSpace(coupon.Status)) coupon.Status = "active";
                if (coupon.Value <= 0) return BadRequest(new { success = false, message = "优惠券数值必须大于0" });
                
                coupon.CreatedAt = DateTime.UtcNow;
                coupon.UsedCount = 0; // 确保初始使用次数为0
                
                _db.Coupons.Add(coupon);
                _db.SaveChanges();
                return Ok(new { success = true, data = coupon });
            }
            catch (Exception ex)
            {
                // 记录详细错误信息
                Console.WriteLine($"创建优惠券失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部异常: {ex.InnerException.Message}");
                }
                try
                {
                    // 尝试在运行时确保表结构存在后重试一次
                    EnsureCouponTables();
                    _db.Coupons.Add(coupon);
                    _db.SaveChanges();
                    return Ok(new { success = true, data = coupon });
                }
                catch (Exception retryEx)
                {
                    Console.WriteLine($"重试创建优惠券失败: {retryEx.Message}");
                    return StatusCode(500, new { success = false, message = "创建优惠券失败", error = retryEx.Message });
                }
            }
        }

        private void EnsureCouponTables()
        {
            try
            {
                _db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS `Coupons` (
                `Id` INT NOT NULL AUTO_INCREMENT,
                `Code` VARCHAR(50) NOT NULL,
                `DiscountType` VARCHAR(20) NOT NULL,
                `Value` DECIMAL(10,2) NOT NULL,
                `MinOrderAmount` DECIMAL(10,2) NOT NULL,
                `MaxDiscount` DECIMAL(10,2) NULL,
                `UsageLimit` INT NULL,
                `UsageLimitPerUser` INT NULL,
                `UsedCount` INT NOT NULL DEFAULT 0,
                `Status` VARCHAR(20) NOT NULL,
                `StartAt` DATETIME NULL,
                `EndAt` DATETIME NULL,
                `CreatedAt` DATETIME NOT NULL,
                `UpdatedAt` DATETIME NULL,
                PRIMARY KEY (`Id`),
                UNIQUE INDEX `IX_Coupons_Code` (`Code`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

                _db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS `UserCoupons` (
                `Id` INT NOT NULL AUTO_INCREMENT,
                `UserId` INT NOT NULL,
                `CouponId` INT NOT NULL,
                `ClaimedAt` DATETIME NOT NULL,
                `UsedAt` DATETIME NULL,
                `Status` VARCHAR(20) NOT NULL,
                PRIMARY KEY (`Id`),
                INDEX `IX_UserCoupons_UserId` (`UserId`),
                INDEX `IX_UserCoupons_CouponId` (`CouponId`),
                UNIQUE INDEX `IX_UserCoupons_UserId_CouponId` (`UserId`, `CouponId`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
            }
            catch {}
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Coupon coupon)
        {
            try
            {
                var db = _db.Coupons.FirstOrDefault(c => c.Id == id);
                if (db == null) return NotFound(new { success = false, message = "优惠券不存在" });
                
                if (!string.IsNullOrWhiteSpace(coupon.Code) && coupon.Code != db.Code)
                {
                    if (_db.Coupons.Any(c => c.Code == coupon.Code)) return BadRequest(new { success = false, message = "Code重复" });
                    db.Code = coupon.Code;
                }
                
                db.DiscountType = coupon.DiscountType;
                db.Value = coupon.Value;
                db.MinOrderAmount = coupon.MinOrderAmount;
                db.MaxDiscount = coupon.MaxDiscount;
                db.UsageLimit = coupon.UsageLimit;
                db.UsageLimitPerUser = coupon.UsageLimitPerUser;
                db.Status = coupon.Status;
                db.StartAt = coupon.StartAt;
                db.EndAt = coupon.EndAt;
                db.UpdatedAt = DateTime.UtcNow;
                
                _db.SaveChanges();
                return Ok(new { success = true, data = db });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新优惠券失败: {ex.Message}");
                return StatusCode(500, new { success = false, message = "更新优惠券失败", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            var db = _db.Coupons.FirstOrDefault(c => c.Id == id);
            if (db == null) return NotFound();
            _db.Coupons.Remove(db);
            _db.SaveChanges();
            return Ok(new { message = "已删除" });
        }

        [HttpGet("{id}/claims")]
        [Authorize(Roles = "admin")]
        public IActionResult Claims(int id, [FromQuery] int page = 1, [FromQuery] int limit = 20)
        {
            var q = _db.UserCoupons.Where(uc => uc.CouponId == id).OrderByDescending(uc => uc.ClaimedAt);
            var total = q.Count();
            var data = q.Skip((page - 1) * limit).Take(limit).ToList();
            return Ok(new { total, data });
        }

        // 发放优惠券给指定用户（管理员）
        [HttpPost("grant")]
        [Authorize(Roles = "admin")]
        public IActionResult GrantToUser([FromBody] GrantCouponRequest req)
        {
            if (req == null || req.UserId <= 0 || req.CouponId <= 0)
                return BadRequest(new { message = "参数不合法" });

            var coupon = _db.Coupons.FirstOrDefault(c => c.Id == req.CouponId);
            if (coupon == null || coupon.Status != "active")
                return NotFound(new { message = "优惠券不存在或未启用" });

            var now = DateTime.UtcNow;
            if ((coupon.StartAt.HasValue && coupon.StartAt > now) || (coupon.EndAt.HasValue && coupon.EndAt < now))
                return BadRequest(new { message = "优惠券不在有效期" });

            var userClaimCount = _db.UserCoupons.Count(x => x.UserId == req.UserId && x.CouponId == req.CouponId);
            if (coupon.UsageLimitPerUser.HasValue && userClaimCount >= coupon.UsageLimitPerUser.Value)
                return BadRequest(new { message = "该用户已达领取上限" });

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                return BadRequest(new { message = "已达总发放上限" });

            var uc = new UserCoupon { UserId = req.UserId, CouponId = req.CouponId, ClaimedAt = now, Status = "claimed" };
            _db.UserCoupons.Add(uc);
            _db.SaveChanges();
            return Ok(new { message = "发放成功", id = uc.Id });
        }

        // 当前用户的优惠券列表
        [HttpGet("my")]
        [Authorize]
        public IActionResult MyCoupons()
        {
            var uidStr = User.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
            if (!int.TryParse(uidStr, out var userId)) return Unauthorized();

            var list = _db.UserCoupons
                .Where(uc => uc.UserId == userId)
                .Join(_db.Coupons, uc => uc.CouponId, c => c.Id, (uc, c) => new {
                    Id = uc.Id,
                    Status = uc.Status,
                    ClaimedAt = uc.ClaimedAt,
                    UsedAt = uc.UsedAt,
                    CouponId = c.Id,
                    Code = c.Code,
                    DiscountType = c.DiscountType,
                    Value = c.Value,
                    MinOrderAmount = c.MinOrderAmount,
                    MaxDiscount = c.MaxDiscount,
                    StartAt = c.StartAt,
                    EndAt = c.EndAt,
                    CouponStatus = c.Status
                })
                .OrderByDescending(x => x.ClaimedAt)
                .ToList();

            return Ok(list);
        }
    }

    public class CouponValidateRequest
    {
        public string Code { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
    }

    public class GrantCouponRequest
    {
        public int UserId { get; set; }
        public int CouponId { get; set; }
    }
}
