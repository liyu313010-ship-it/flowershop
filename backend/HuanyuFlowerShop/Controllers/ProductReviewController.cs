using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.Exceptions;
using System.Security.Claims;
using System.Collections.Generic;

namespace HuanyuFlowerShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductReviewController : ControllerBase
    {
        private readonly IProductReviewService _productReviewService;
        private readonly ILogger<ProductReviewController> _logger;

        public ProductReviewController(IProductReviewService productReviewService, ILogger<ProductReviewController> logger)
        {
            _productReviewService = productReviewService;
            _logger = logger;
        }

        /// <summary>
        /// 获取指定产品的评价列表
        /// </summary>
        /// <param name="productId">产品ID</param>
        /// <param name="pageNumber">页码，默认1</param>
        /// <param name="pageSize">每页数量，默认10</param>
        /// <returns>评价列表响应</returns>
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<ProductReviewListResponse>> GetProductReviews(
            int productId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var reviews = await _productReviewService.GetProductReviewsAsync(productId, pageNumber, pageSize);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取产品评价列表失败: {Message}", ex.Message);
                return StatusCode(500, new { message = "获取评价列表失败" });
            }
        }

        /// <summary>
        /// 获取所有评价（首页展示）
        /// </summary>
        /// <param name="pageNumber">页码，默认1</param>
        /// <param name="pageSize">每页数量，默认10</param>
        /// <returns>评价列表</returns>
        [HttpGet("all")]
        public async Task<ActionResult<List<ProductReviewDto>>> GetAllReviews(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var reviews = await _productReviewService.GetAllReviewsAsync(pageNumber, pageSize);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取所有评价失败: {Message}", ex.Message);
                return Ok(new List<ProductReviewDto>());
            }
        }

        /// <summary>
        /// 获取指定产品的平均评分
        /// </summary>
        /// <param name="productId">产品ID</param>
        /// <returns>平均评分</returns>
        [HttpGet("product/{productId}/average-rating")]
        public async Task<ActionResult<double>> GetAverageRating(int productId)
        {
            try
            {
                var averageRating = await _productReviewService.GetProductAverageRatingAsync(productId);
                return Ok(new { productId, averageRating });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取产品平均评分失败: {Message}", ex.Message);
                return StatusCode(500, new { message = "获取平均评分失败" });
            }
        }

        [HttpGet("{reviewId}")]
        public async Task<ActionResult<ProductReviewDto>> GetReviewById(int reviewId)
        {
            try
            {
                var review = await _productReviewService.GetReviewByIdAsync(reviewId);
                return review != null ? Ok(review) : NotFound(new { message = "评价不存在" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取评价失败: {Message}", ex.Message);
                return StatusCode(500, new { message = "获取评价失败" });
            }
        }

        /// <summary>
        /// 创建产品评价（需要登录）
        /// </summary>
        /// <param name="request">创建评价请求</param>
        /// <returns>创建的评价</returns>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ProductReviewDto>> CreateReview([FromBody] CreateProductReviewRequest request)
        {
            try
            {
                // 获取当前登录用户ID
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }
                var userId = int.Parse(userIdClaim);
                
                var review = await _productReviewService.CreateReviewAsync(userId, request);
                return CreatedAtAction(nameof(GetProductReviews), new { productId = review.ProductId }, review);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "创建评价时找不到相关实体: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "创建评价业务逻辑错误: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建评价失败: {Message}", ex.Message);
                return StatusCode(500, new { message = "创建评价失败" });
            }
        }

        /// <summary>
        /// 更新产品评价（需要登录）
        /// </summary>
        /// <param name="reviewId">评价ID</param>
        /// <param name="request">更新评价请求</param>
        /// <returns>更新后的评价</returns>
        [Authorize]
        [HttpPut("{reviewId}")]
        public async Task<ActionResult<ProductReviewDto>> UpdateReview(int reviewId, [FromBody] UpdateProductReviewRequest request)
        {
            try
            {
                // 获取当前登录用户ID
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }
                var userId = int.Parse(userIdClaim);
                
                var review = await _productReviewService.UpdateReviewAsync(reviewId, userId, request);
                return Ok(review);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "更新评价时找不到相关实体: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex, "更新评价未授权: {Message}", ex.Message);
                return Forbid();
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "更新评价业务逻辑错误: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新评价失败: {Message}", ex.Message);
                return StatusCode(500, new { message = "更新评价失败" });
            }
        }

        /// <summary>
        /// 删除产品评价（需要登录）
        /// </summary>
        /// <param name="reviewId">评价ID</param>
        /// <returns>删除结果</returns>
        [Authorize]
        [HttpDelete("{reviewId}")]
        public async Task<ActionResult>
            DeleteReview(int reviewId)
        {
            try
            {
                // 获取当前登录用户ID
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }
                var userId = int.Parse(userIdClaim);
                var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
                if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    await _productReviewService.DeleteReviewByAdminAsync(reviewId);
                }
                else
                {
                    await _productReviewService.DeleteReviewAsync(reviewId, userId);
                }
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "删除评价时找不到相关实体: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex, "删除评价未授权: {Message}", ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除评价失败: {Message}", ex.Message);
                return StatusCode(500, new { message = "删除评价失败" });
            }
        }

        /// <summary>
        /// 获取当前用户对指定产品的评价
        /// </summary>
        /// <param name="productId">产品ID</param>
        /// <returns>用户的评价</returns>
        [Authorize]
        [HttpGet("user/product/{productId}")]
        public async Task<ActionResult<ProductReviewDto>> GetUserReviewForProduct(int productId)
        {
            try
            {
                // 获取当前登录用户ID
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdStr == null)
                {
                    throw new UnauthorizedException("用户未认证");
                }
                var userId = int.Parse(userIdStr);
                
                var review = await _productReviewService.GetUserReviewForProductAsync(userId, productId);
                return review != null ? Ok(review) : NotFound(new { message = "用户未评价此产品" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户评价失败: {Message}", ex.Message);
                return StatusCode(500, new { message = "获取用户评价失败" });
            }
        }

        
    }
}
