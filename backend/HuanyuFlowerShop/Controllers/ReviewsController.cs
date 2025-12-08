using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.DTOs;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace HuanyuFlowerShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// 获取产品的评价列表
        /// </summary>
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetProductReviews(int productId)
        {
            try
            {
                _logger.LogInformation("获取产品 {ProductId} 的评价列表", productId);
                
                var reviews = await _reviewService.GetProductReviewsAsync(productId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取产品评价列表失败");
                return StatusCode(500, "获取评价列表时发生错误");
            }
        }

        /// <summary>
        /// 获取评价详情
        /// </summary>
        [HttpGet("{reviewId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ReviewDto>> GetReview(int reviewId)
        {
            try
            {
                _logger.LogInformation("获取评价ID {ReviewId} 的详情", reviewId);
                
                var review = await _reviewService.GetReviewByIdAsync(reviewId);
                if (review == null)
                {
                    _logger.LogWarning("评价 {ReviewId} 不存在", reviewId);
                    return NotFound("评价不存在");
                }
                
                return Ok(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取评价详情失败");
                return StatusCode(500, "获取评价详情时发生错误");
            }
        }

        /// <summary>
        /// 创建新评价
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto createReviewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation("用户 {UserId} 创建新评价", userId);
                
                var review = await _reviewService.CreateReviewAsync(userId, createReviewDto);
                return CreatedAtAction(nameof(GetReview), new { reviewId = review.Id }, review);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "创建评价失败，参数无效");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建评价时发生错误");
                return StatusCode(500, "创建评价时发生错误");
            }
        }

        /// <summary>
        /// 更新评价
        /// </summary>
        [HttpPut("{reviewId}")]
        [Authorize]
        public async Task<ActionResult<ReviewDto>> UpdateReview(int reviewId, [FromBody] UpdateReviewDto updateReviewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation("用户 {UserId} 更新评价 {ReviewId}", userId, reviewId);
                
                var review = await _reviewService.UpdateReviewAsync(userId, reviewId, updateReviewDto);
                if (review == null)
                {
                    return NotFound("评价不存在");
                }
                
                return Ok(review);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "用户尝试更新不属于自己的评价");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新评价时发生错误");
                return StatusCode(500, "更新评价时发生错误");
            }
        }

        /// <summary>
        /// 删除评价
        /// </summary>
        [HttpDelete("{reviewId}")]
        [Authorize]
        public async Task<ActionResult> DeleteReview(int reviewId)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("用户 {UserId} 删除评价 {ReviewId}", userId, reviewId);
                
                var success = await _reviewService.DeleteReviewAsync(userId, reviewId);
                if (!success)
                {
                    return NotFound("评价不存在");
                }
                
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "用户尝试删除不属于自己的评价");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除评价时发生错误");
                return StatusCode(500, "删除评价时发生错误");
            }
        }

        /// <summary>
        /// 获取产品的平均评分
        /// </summary>
        [HttpGet("product/{productId}/average-rating")]
        [AllowAnonymous]
        public async Task<ActionResult<decimal>> GetAverageProductRating(int productId)
        {
            try
            {
                _logger.LogInformation("获取产品 {ProductId} 的平均评分", productId);
                
                var averageRating = await _reviewService.GetAverageProductRatingAsync(productId);
                return Ok(averageRating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取产品平均评分失败");
                return StatusCode(500, "获取平均评分时发生错误");
            }
        }

        /// <summary>
        /// 获取产品的评价数量
        /// </summary>
        [HttpGet("product/{productId}/count")]
        [AllowAnonymous]
        public async Task<ActionResult<int>> GetProductReviewCount(int productId)
        {
            try
            {
                _logger.LogInformation("获取产品 {ProductId} 的评价数量", productId);
                
                var reviewCount = await _reviewService.GetProductReviewCountAsync(productId);
                return Ok(reviewCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取产品评价数量失败");
                return StatusCode(500, "获取评价数量时发生错误");
            }
        }

        /// <summary>
        /// 检查当前用户是否已评价产品
        /// </summary>
        [HttpGet("product/{productId}/has-reviewed")]
        [Authorize]
        public async Task<ActionResult<bool>> HasUserReviewedProduct(int productId)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("检查用户 {UserId} 是否评价过产品 {ProductId}", userId, productId);
                
                var hasReviewed = await _reviewService.HasUserReviewedProductAsync(userId, productId);
                return Ok(hasReviewed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查用户是否已评价产品失败");
                return StatusCode(500, "检查评价状态时发生错误");
            }
        }

        /// <summary>
        /// 获取当前认证用户的ID
        /// </summary>
        private int GetCurrentUserId()
        {
            // 假设JWT token中包含用户ID声明
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("无法识别用户身份");
            }
            
            return int.Parse(userIdClaim);
        }
    }
}
