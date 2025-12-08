using HuanyuFlowerShop.DTOs;
using System.Threading.Tasks;

namespace HuanyuFlowerShop.Services
{
    public interface IFavoriteService
    {
        /// <summary>
        /// 添加收藏
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="productId">商品ID</param>
        /// <returns>是否添加成功</returns>
        Task<bool> AddFavoriteAsync(int userId, int productId);

        /// <summary>
        /// 移除收藏
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="productId">商品ID</param>
        /// <returns>是否移除成功</returns>
        Task<bool> RemoveFavoriteAsync(int userId, int productId);

        /// <summary>
        /// 检查是否已收藏
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="productId">商品ID</param>
        /// <returns>是否已收藏</returns>
        Task<bool> IsProductFavoritedAsync(int userId, int productId);

        /// <summary>
        /// 获取用户收藏列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>收藏列表</returns>
        Task<(IEnumerable<FavoriteDto>, int)> GetUserFavoritesAsync(int userId, int page = 1, int pageSize = 10);
    }
}