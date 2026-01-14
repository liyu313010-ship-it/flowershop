using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.DTOs;
using System.Security.Claims;

namespace HuanyuFlowerShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartsController(ICartService cartService, ILogger<CartsController> logger) : ControllerBase
    {
        private readonly ICartService _cartService = cartService;
        private readonly ILogger<CartsController> _logger = logger;

        /// <summary>
        /// 获取当前用户的购物车商品列表
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItemDto>>> GetCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("用户 {UserId} 获取购物车列表", userId);
                
                var cartItems = await _cartService.GetCartItemsAsync(userId);
                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取购物车列表失败");
                return StatusCode(500, "获取购物车列表时发生错误");
            }
        }

        /// <summary>
        /// 添加商品到购物车
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CartItemDto>> AddToCart([FromBody] AddToCartDto addToCartDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation("用户 {UserId} 添加商品 {ProductId} 到购物车，数量: {Quantity}", 
                    userId, addToCartDto.ProductId, addToCartDto.Quantity);
                
                var cartItem = await _cartService.AddToCartAsync(userId, addToCartDto);
                return CreatedAtAction(nameof(GetCart), cartItem);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "添加商品到购物车失败");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加商品到购物车时发生错误");
                return StatusCode(500, "添加商品到购物车时发生错误");
            }
        }

        /// <summary>
        /// 更新购物车商品数量
        /// </summary>
        [HttpPut("{cartItemId}")]
        public async Task<ActionResult<CartItemDto>> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemDto updateCartItemDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation("用户 {UserId} 更新购物车商品 {CartItemId}，数量: {Quantity}", 
                    userId, cartItemId, updateCartItemDto.Quantity);
                
                var cartItem = await _cartService.UpdateCartItemAsync(userId, cartItemId, updateCartItemDto);
                if (cartItem == null)
                {
                    _logger.LogWarning("购物车商品 {CartItemId} 不存在或不属于用户 {UserId}", cartItemId, userId);
                    return NotFound("购物车商品不存在或不属于当前用户");
                }
                
                return Ok(cartItem);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "更新购物车商品失败");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新购物车商品时发生错误");
                return StatusCode(500, "更新购物车商品时发生错误");
            }
        }

        /// <summary>
        /// 从购物车移除商品
        /// </summary>
        [HttpDelete("{cartItemId}")]
        public async Task<ActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("用户 {UserId} 从购物车移除商品 {CartItemId}", userId, cartItemId);
                
                var success = await _cartService.RemoveFromCartAsync(userId, cartItemId);
                if (!success)
                {
                    _logger.LogWarning("购物车商品 {CartItemId} 不存在或不属于用户 {UserId}", cartItemId, userId);
                    return NotFound("购物车商品不存在或不属于当前用户");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从购物车移除商品时发生错误");
                return StatusCode(500, "从购物车移除商品时发生错误");
            }
        }

        /// <summary>
        /// 清空购物车
        /// </summary>
        [HttpDelete]
        public async Task<ActionResult> ClearCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("用户 {UserId} 清空购物车", userId);
                
                await _cartService.ClearCartAsync(userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空购物车时发生错误");
                return StatusCode(500, "清空购物车时发生错误");
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
