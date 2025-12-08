using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HuanyuFlowerShop.Exceptions;

namespace HuanyuFlowerShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        /// <returns>用户ID</returns>
        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null)
            {
                throw new UnauthorizedException("用户未认证");
            }
            return int.Parse(userIdStr);
        }

        /// <summary>
        /// 添加收藏
        /// </summary>
        /// <param name="dto">添加收藏请求</param>
        /// <returns>操作结果</returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Success = false, Message = "请求参数无效" });
            }

            int userId = GetCurrentUserId();
            bool result = await _favoriteService.AddFavoriteAsync(userId, dto.ProductId);

            if (result)
            {
                return Ok(new { Success = true, Message = "收藏成功" });
            }
            else
            {
                return BadRequest(new { Success = false, Message = "已经收藏过该商品" });
            }
        }

        /// <summary>
        /// 移除收藏
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns>操作结果</returns>
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFavorite(int productId)
        {
            int userId = GetCurrentUserId();
            bool result = await _favoriteService.RemoveFavoriteAsync(userId, productId);

            if (result)
            {
                return Ok(new { Success = true, Message = "取消收藏成功" });
            }
            else
            {
                return BadRequest(new { Success = false, Message = "未找到收藏记录" });
            }
        }

        /// <summary>
        /// 检查是否已收藏
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns>收藏状态</returns>
        [HttpGet("check/{productId}")]
        public async Task<IActionResult> CheckFavorite(int productId)
        {
            int userId = GetCurrentUserId();
            bool isFavorited = await _favoriteService.IsProductFavoritedAsync(userId, productId);

            return Ok(new { IsFavorited = isFavorited });
        }

        /// <summary>
        /// 获取用户收藏列表
        /// </summary>
        /// <param name="page">页码，默认为1</param>
        /// <param name="pageSize">每页数量，默认为10</param>
        /// <returns>收藏列表</returns>
        [HttpGet("list")]
        public async Task<IActionResult> GetFavorites([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { Success = false, Message = "页码或每页数量无效" });
            }

            int userId = GetCurrentUserId();
            var (favorites, totalCount) = await _favoriteService.GetUserFavoritesAsync(userId, page, pageSize);

            return Ok(new FavoriteListResponseDto
            {
                Success = true,
                Message = "获取成功",
                Favorites = favorites,
                TotalCount = totalCount
            });
        }
    }
}