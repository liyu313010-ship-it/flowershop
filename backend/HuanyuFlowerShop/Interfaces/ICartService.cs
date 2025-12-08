using HuanyuFlowerShop.DTOs;

namespace HuanyuFlowerShop.Interfaces
{
    /// <summary>
    /// 购物车服务接口
    /// 提供购物车相关的核心功能
    /// </summary>
    public interface ICartService
    {
        /// <summary>
        /// 获取用户的购物车商品列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>购物车商品列表</returns>
        Task<IEnumerable<CartItemDto>> GetCartItemsAsync(int userId);
        
        /// <summary>
        /// 添加商品到购物车
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="addToCartDto">添加购物车的数据</param>
        /// <returns>添加的购物车商品</returns>
        Task<CartItemDto> AddToCartAsync(int userId, AddToCartDto addToCartDto);
        
        /// <summary>
        /// 更新购物车商品数量
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="cartItemId">购物车商品ID</param>
        /// <param name="updateCartItemDto">更新的数据</param>
        /// <returns>更新后的购物车商品</returns>
        Task<CartItemDto?> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemDto updateCartItemDto);
        
        /// <summary>
        /// 从购物车移除商品
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="cartItemId">购物车商品ID</param>
        /// <returns>移除是否成功</returns>
        Task<bool> RemoveFromCartAsync(int userId, int cartItemId);
        
        /// <summary>
        /// 清空购物车
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>清空是否成功</returns>
        Task<bool> ClearCartAsync(int userId);
    }
}