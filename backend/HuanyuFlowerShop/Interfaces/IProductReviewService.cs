using System.Threading.Tasks;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Entities;

namespace HuanyuFlowerShop.Interfaces
{
    /// <summary>
    /// 产品评价服务接口
    /// </summary>
    public interface IProductReviewService
    {
        /// <summary>
        /// 创建产品评价
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="request">创建请求</param>
        /// <returns>创建的评价</returns>
        Task<ProductReviewDto> CreateReviewAsync(int userId, CreateProductReviewRequest request);

        /// <summary>
        /// 更新产品评价
        /// </summary>
        /// <param name="reviewId">评价ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的评价</returns>
        Task<ProductReviewDto> UpdateReviewAsync(int reviewId, int userId, UpdateProductReviewRequest request);

        /// <summary>
        /// 删除产品评价
        /// </summary>
        /// <param name="reviewId">评价ID</param>
        /// <param name="userId">用户ID</param>
        Task DeleteReviewAsync(int reviewId, int userId);
        
        /// <summary>
        /// 获取用户对特定产品的评价
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="productId">产品ID</param>
        /// <returns>评价信息，如无则返回null</returns>
        Task<ProductReviewDto?> GetUserReviewForProductAsync(int userId, int productId);
        
        /// <summary>
        /// 获取产品评价列表
        /// </summary>
        /// <param name="productId">产品ID</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>评价列表响应</returns>
        Task<ProductReviewListResponse> GetProductReviewsAsync(int productId, int pageIndex = 1, int pageSize = 10);

        /// <summary>
        /// 获取用户对产品的评价
        /// </summary>
        /// <param name="productId">产品ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns>用户评价</returns>
        Task<ProductReviewDto?> GetUserProductReviewAsync(int productId, int userId);

        Task<ProductReviewDto?> GetReviewByIdAsync(int reviewId);

        Task DeleteReviewByAdminAsync(int reviewId);

        /// <summary>
        /// 获取产品平均评分
        /// </summary>
        /// <param name="productId">产品ID</param>
        /// <returns>平均评分</returns>
        Task<double> GetProductAverageRatingAsync(int productId);

        /// <summary>
        /// 获取所有评价（用于首页展示）
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>评价列表</returns>
        Task<List<ProductReviewDto>> GetAllReviewsAsync(int pageIndex = 1, int pageSize = 10);
    }
}
