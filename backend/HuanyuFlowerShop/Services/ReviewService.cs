using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HuanyuFlowerShop.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(ApplicationDbContext context, ILogger<ReviewService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId)
        {
            try
            {
                _logger.LogInformation("获取产品 {ProductId} 的评价列表", productId);
                
                return await _context.Reviews
                    .Where(r => r.ProductId == productId)
                    .Include(r => r.User)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ReviewDto
                    {
                        Id = r.Id,
                        ProductId = r.ProductId,
                        UserId = r.UserId,
                        UserName = (r.User != null ? r.User.Username : string.Empty),
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取产品评价列表失败");
                throw;
            }
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int reviewId)
        {
            try
            {
                _logger.LogInformation("获取评价ID {ReviewId} 的详情", reviewId);
                
                var review = await _context.Reviews
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Id == reviewId);

                if (review == null)
                {
                    return null;
                }

                return new ReviewDto
                {
                    Id = review.Id,
                    ProductId = review.ProductId,
                    UserId = review.UserId,
                    UserName = review.User?.Username ?? string.Empty,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取评价详情失败");
                throw;
            }
        }

        public async Task<ReviewDto> CreateReviewAsync(int userId, CreateReviewDto createReviewDto)
        {
            try
            {
                // 检查产品是否存在
                var productExists = await _context.Products.AnyAsync(p => p.Id == createReviewDto.ProductId);
                if (!productExists)
                {
                    throw new ArgumentException("产品不存在");
                }

                // 检查用户是否已经评价过该产品
                var hasReviewed = await HasUserReviewedProductAsync(userId, createReviewDto.ProductId);
                if (hasReviewed)
                {
                    throw new ArgumentException("您已经评价过该产品");
                }

                // 检查用户是否购买过该产品（可选）
                var hasPurchased = await _context.OrderItems
                    .Where(oi => oi.ProductId == createReviewDto.ProductId)
                    .AnyAsync(oi => oi.Order!.UserId == userId && oi.Order.Status == "delivered");
                
                if (!hasPurchased)
                {
                    throw new ArgumentException("只有购买过该产品的用户才能评价");
                }

                var review = new Review
                {
                    ProductId = createReviewDto.ProductId,
                    UserId = userId,
                    Rating = createReviewDto.Rating,
                    Comment = createReviewDto.Comment,
                    CreatedAt = DateTime.Now
                };

                await _context.Reviews.AddAsync(review);
                await _context.SaveChangesAsync();

                _logger.LogInformation("用户 {UserId} 为产品 {ProductId} 创建评价", userId, createReviewDto.ProductId);

                // 获取用户信息
                var user = await _context.Users.FindAsync(userId);

                return new ReviewDto
                {
                    Id = review.Id,
                    ProductId = review.ProductId,
                    UserId = review.UserId,
                    UserName = user?.Username ?? string.Empty,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt
                };
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "创建评价失败，参数无效");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建评价时发生错误");
                throw;
            }
        }

        public async Task<ReviewDto?> UpdateReviewAsync(int userId, int reviewId, UpdateReviewDto updateReviewDto)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    return null;
                }

                // 检查是否是评价作者
                if (review.UserId != userId)
                {
                    throw new UnauthorizedAccessException("您无权修改此评价");
                }

                review.Rating = updateReviewDto.Rating;
                review.Comment = updateReviewDto.Comment;
                review.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("用户 {UserId} 更新评价 {ReviewId}", userId, reviewId);

                // 获取用户信息
                var user = await _context.Users.FindAsync(userId);

                return new ReviewDto
                {
                    Id = review.Id,
                    ProductId = review.ProductId,
                    UserId = review.UserId,
                    UserName = user?.Username ?? string.Empty,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt
                };
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "用户尝试更新不属于自己的评价");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新评价失败");
                throw;
            }
        }

        public async Task<bool> DeleteReviewAsync(int userId, int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    return false;
                }

                // 检查是否是评价作者
                if (review.UserId != userId)
                {
                    throw new UnauthorizedAccessException("您无权删除此评价");
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                _logger.LogInformation("用户 {UserId} 删除评价 {ReviewId}", userId, reviewId);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "用户尝试删除不属于自己的评价");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除评价失败");
                throw;
            }
        }

        public async Task<decimal> GetAverageProductRatingAsync(int productId)
        {
            try
            {
                var averageRating = await _context.Reviews
                    .Where(r => r.ProductId == productId)
                    .AverageAsync(r => (decimal)r.Rating);

                return Math.Round(averageRating, 1);
            }
            catch (InvalidOperationException)
            {
                // 没有评价时返回0
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取产品平均评分失败");
                throw;
            }
        }

        public async Task<int> GetProductReviewCountAsync(int productId)
        {
            try
            {
                return await _context.Reviews
                    .CountAsync(r => r.ProductId == productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取产品评价数量失败");
                throw;
            }
        }

        public async Task<bool> HasUserReviewedProductAsync(int userId, int productId)
        {
            try
            {
                return await _context.Reviews
                    .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查用户是否已评价产品失败");
                throw;
            }
        }
    }
}
