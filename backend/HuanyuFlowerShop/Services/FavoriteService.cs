using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HuanyuFlowerShop.Services
{
    public class FavoriteService(ApplicationDbContext context) : IFavoriteService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<bool> AddFavoriteAsync(int userId, int productId)
        {
            // 检查是否已经收藏
            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (existingFavorite != null)
            {
                // 已经收藏过
                return false;
            }

            // 检查商品是否存在
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return false;
            }

            // 添加收藏
            var favorite = new Favorite
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            try
            {
                var count = await _context.Favorites.CountAsync(f => f.ProductId == productId);
                await _context.Database.ExecuteSqlRawAsync("UPDATE Products SET Popularity = {0} WHERE Id = {1}", count, productId);
            }
            catch { }
            return true;
        }

        public async Task<bool> RemoveFavoriteAsync(int userId, int productId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (favorite == null)
            {
                return false;
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            try
            {
                var count = await _context.Favorites.CountAsync(f => f.ProductId == productId);
                await _context.Database.ExecuteSqlRawAsync("UPDATE Products SET Popularity = {0} WHERE Id = {1}", count, productId);
            }
            catch { }
            return true;
        }

        public async Task<bool> IsProductFavoritedAsync(int userId, int productId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.ProductId == productId);
        }

        public async Task<(IEnumerable<FavoriteDto>, int)> GetUserFavoritesAsync(int userId, int page = 1, int pageSize = 10)
        {
            // 计算总数
            var totalCount = await _context.Favorites.CountAsync(f => f.UserId == userId);

            // 获取分页数据
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Product)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FavoriteDto
                {
                    Id = f.Id,
                    ProductId = f.ProductId,
                    ProductName = (f.Product != null ? f.Product.Name : "未知商品"),
                    ProductImage = (f.Product != null ? f.Product.ImageUrl : string.Empty),
                    ProductPrice = (f.Product != null ? f.Product.Price : 0),
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();

            return (favorites, totalCount);
        }
    }
}
