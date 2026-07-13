using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HuanyuFlowerShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public RecommendationsController(ApplicationDbContext db) { _db = db; }

        [HttpGet("global")]
        public IActionResult GetGlobal([FromQuery] int limit = 8)
        {
            // 简单权重：销量 + 收藏数
            var favorites = _db.Favorites.GroupBy(f => f.ProductId).Select(g => new { ProductId = g.Key, C = g.Count() }).ToDictionary(x => x.ProductId, x => x.C);
            var products = _db.Products.ToList();
            var scored = products.Select(p => new { p.Id, Score = (decimal)(p.SalesCount) + (favorites.ContainsKey(p.Id) ? favorites[p.Id] * 0.5m : 0m) }).OrderByDescending(x => x.Score).Take(limit).ToList();
            // 写入推荐表（覆盖旧记录）
            var old = _db.ProductRecommendations.Where(r => r.ForUserId == null).ToList();
            _db.ProductRecommendations.RemoveRange(old);
            foreach (var s in scored)
            {
                _db.ProductRecommendations.Add(new ProductRecommendation { ProductId = s.Id, ForUserId = null, Score = s.Score, GeneratedAt = DateTime.UtcNow });
            }
            _db.SaveChanges();
            var result = scored.Select(s => new { productId = s.Id, score = s.Score }).ToList();
            return Ok(result);
        }

        [HttpGet("user")]
        [Authorize]
        public IActionResult GetForUser([FromQuery] int limit = 8)
        {
            var uidStr = User.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
            if (!int.TryParse(uidStr, out var userId)) return Unauthorized();
            var favs = _db.Favorites.Where(f => f.UserId == userId).GroupBy(f => f.ProductId).Select(g => new { ProductId = g.Key, C = g.Count() }).ToDictionary(x => x.ProductId, x => x.C);
            var userOrderIds = _db.Orders.Where(o => o.UserId == userId).Select(o => o.Id).ToList();
            var orderItems = _db.OrderItems.Where(oi => userOrderIds.Contains(oi.OrderId)).GroupBy(oi => oi.ProductId).Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) }).ToDictionary(x => x.ProductId, x => x.Qty);
            var products = _db.Products.ToList();
            var scored = products.Select(p => new { p.Id, Score = (orderItems.ContainsKey(p.Id) ? orderItems[p.Id] * 1.0m : 0m) + (favs.ContainsKey(p.Id) ? favs[p.Id] * 2.0m : 0m) + p.SalesCount * 0.2m })
                .OrderByDescending(x => x.Score).Take(limit).ToList();
            // 写入推荐表（用户维度）
            var old = _db.ProductRecommendations.Where(r => r.ForUserId == userId).ToList();
            _db.ProductRecommendations.RemoveRange(old);
            foreach (var s in scored)
            {
                _db.ProductRecommendations.Add(new ProductRecommendation { ProductId = s.Id, ForUserId = userId, Score = s.Score, GeneratedAt = DateTime.UtcNow });
            }
            _db.SaveChanges();
            var result = scored.Select(s => new { productId = s.Id, score = s.Score }).ToList();
            return Ok(result);
        }
    }
}
