using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.Exceptions;

namespace HuanyuFlowerShop.Services
{
    /// <summary>
    /// 产品评价服务实现
    /// </summary>
    public class ProductReviewService : IProductReviewService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context">数据库上下文</param>
        public ProductReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 创建产品评价
        /// </summary>
        public async Task<ProductReviewDto> CreateReviewAsync(int userId, CreateProductReviewRequest request)
        {
            // 检查产品是否存在
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                throw new ArgumentException("产品不存在");
            }

            // 仅允许对已送达的订单中的商品进行评价
            var canReview = await _context.Orders
                .Where(o => o.UserId == userId && o.Status == "delivered")
                .AnyAsync(o => _context.OrderItems.Any(oi => oi.OrderId == o.Id && oi.ProductId == request.ProductId));

            if (!canReview)
            {
                throw new BusinessException("仅已送达的订单商品可评价");
            }

            // 检查用户是否已经评价过该产品
            var existingReview = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.ProductId == request.ProductId && r.UserId == userId && !r.IsDeleted);

            if (existingReview != null)
            {
                throw new ArgumentException("您已经评价过该产品");
            }

            // 创建新评价
            var review = new ProductReview
            {
                ProductId = request.ProductId,
                UserId = userId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.Now
            };

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            // 获取用户信息
            var user = await _context.Users.FindAsync(userId);

            return new ProductReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                UserId = review.UserId,
                UserName = user?.Username ?? "匿名用户",
                Avatar = user?.Avatar,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }

        /// <summary>
        /// 更新产品评价
        /// </summary>
        public async Task<ProductReviewDto> UpdateReviewAsync(int reviewId, int userId, UpdateProductReviewRequest request)
        {
            var review = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId && !r.IsDeleted);

            if (review == null)
            {
                throw new ArgumentException("评价不存在或无权限修改");
            }

            // 更新评价
            review.Rating = request.Rating;
            review.Comment = request.Comment;
            review.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // 获取用户信息
            var user = await _context.Users.FindAsync(userId);

            return new ProductReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                UserId = review.UserId,
                UserName = user?.Username ?? "匿名用户",
                Avatar = user?.Avatar,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }

        /// <summary>
        /// 删除产品评价
        /// </summary>
        public async Task DeleteReviewAsync(int reviewId, int userId)
        {
            var review = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId && !r.IsDeleted);

            if (review == null)
            {
                throw new EntityNotFoundException("ProductReview", reviewId);
            }

            if (review.UserId != userId)
            {
                throw new UnauthorizedException("用户无权删除此评价");
            }

            // 软删除
            review.IsDeleted = true;
            review.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteReviewByAdminAsync(int reviewId)
        {
            var review = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);

            if (review == null)
            {
                throw new EntityNotFoundException("ProductReview", reviewId);
            }

            review.IsDeleted = true;
            review.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task<ProductReviewDto?> GetUserReviewForProductAsync(int userId, int productId)
        {
            var review = await _context.ProductReviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId && !r.IsDeleted);
            
            if (review == null)
            {
                return null;
            }

            return new ProductReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                UserId = review.UserId,
                UserName = review.User?.Username ?? "匿名用户",
                Avatar = review.User?.Avatar,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<ProductReviewDto?> GetUserProductReviewAsync(int productId, int userId)
        {
            // 这两个方法功能相同，只是参数顺序不同
            return await GetUserReviewForProductAsync(userId, productId);
        }

        public async Task<ProductReviewDto?> GetReviewByIdAsync(int reviewId)
        {
            var review = await _context.ProductReviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);

            if (review == null)
            {
                return null;
            }

            return new ProductReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                UserId = review.UserId,
                UserName = review.User?.Username ?? "匿名用户",
                Avatar = review.User?.Avatar,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<double> GetProductAverageRatingAsync(int productId)
        {
            // 检查产品是否存在
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new ArgumentException("产品不存在");
            }

            // 获取所有未删除的评价
            var reviews = _context.ProductReviews
                .Where(r => r.ProductId == productId && !r.IsDeleted);

            var totalCount = await reviews.CountAsync();
            if (totalCount == 0)
            {
                return 0;
            }

            // 计算平均评分
            return await reviews.AverageAsync(r => r.Rating);
        }

        /// <summary>
        /// 获取产品评价列表
        /// </summary>
        public async Task<ProductReviewListResponse> GetProductReviewsAsync(int productId, int pageIndex = 1, int pageSize = 10)
        {
            // 检查产品是否存在
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new ArgumentException("产品不存在");
            }

            // 获取所有评价（未删除的）
            var reviews = _context.ProductReviews
                .Where(r => r.ProductId == productId && !r.IsDeleted)
                .Include(r => r.User);

            // 计算总数
            var totalCount = await reviews.CountAsync();

            // 获取分页数据
            var reviewList = await reviews
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 计算星级统计
            var allReviews = await reviews.ToListAsync();
            var fiveStarCount = allReviews.Count(r => r.Rating == 5);
            var fourStarCount = allReviews.Count(r => r.Rating == 4);
            var threeStarCount = allReviews.Count(r => r.Rating == 3);
            var twoStarCount = allReviews.Count(r => r.Rating == 2);
            var oneStarCount = allReviews.Count(r => r.Rating == 1);

            // 计算平均评分
            var averageRating = totalCount > 0 ? allReviews.Average(r => r.Rating) : 0;

            // 转换为DTO
            var items = reviewList.Select(r => new ProductReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                UserId = r.UserId,
                UserName = r.User?.Username ?? "匿名用户",
                Avatar = r.User?.Avatar,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList();

            return new ProductReviewListResponse
            {
                Items = items,
                TotalCount = totalCount,
                AverageRating = Math.Round(averageRating, 1),
                FiveStarCount = fiveStarCount,
                FourStarCount = fourStarCount,
                ThreeStarCount = threeStarCount,
                TwoStarCount = twoStarCount,
                OneStarCount = oneStarCount
            };
        }

        /// <summary>
        /// 获取所有评价（用于首页展示）
        /// </summary>
        public async Task<List<ProductReviewDto>> GetAllReviewsAsync(int pageIndex = 1, int pageSize = 10)
        {
            // 获取所有未删除的评价，按创建时间倒序排序
            var reviews = await _context.ProductReviews
                .Where(r => !r.IsDeleted)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 转换为DTO
            return reviews.Select(r => new ProductReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                UserId = r.UserId,
                UserName = r.User?.Username ?? "匿名用户",
                Avatar = r.User?.Avatar,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList();
        }
    }
}
