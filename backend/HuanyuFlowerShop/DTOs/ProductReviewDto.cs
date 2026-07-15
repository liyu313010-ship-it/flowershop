using System;

namespace HuanyuFlowerShop.DTOs
{
    /// <summary>
    /// 产品评价DTO
    /// </summary>
    public class ProductReviewDto
    {
        /// <summary>
        /// 评价ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 产品ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 产品名称，供评价列表直接链接到对应商品
        /// </summary>
        public string? ProductName { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 用户头像（相对路径或完整URL）
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// 评价内容
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 创建产品评价请求DTO
    /// </summary>
    public class CreateProductReviewRequest
    {
        /// <summary>
        /// 产品ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 评分（1-5）
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// 评价内容
        /// </summary>
        public string? Comment { get; set; }
    }

    /// <summary>
    /// 更新产品评价请求DTO
    /// </summary>
    public class UpdateProductReviewRequest
    {
        /// <summary>
        /// 评分（1-5）
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// 评价内容
        /// </summary>
        public string? Comment { get; set; }
    }

    /// <summary>
    /// 产品评价列表响应DTO
    /// </summary>
    public class ProductReviewListResponse
    {
        /// <summary>
        /// 评价列表
        /// </summary>
        public List<ProductReviewDto> Items { get; set; } = new List<ProductReviewDto>();

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 平均评分
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// 5星评价数量
        /// </summary>
        public int FiveStarCount { get; set; }

        /// <summary>
        /// 4星评价数量
        /// </summary>
        public int FourStarCount { get; set; }

        /// <summary>
        /// 3星评价数量
        /// </summary>
        public int ThreeStarCount { get; set; }

        /// <summary>
        /// 2星评价数量
        /// </summary>
        public int TwoStarCount { get; set; }

        /// <summary>
        /// 1星评价数量
        /// </summary>
        public int OneStarCount { get; set; }
    }
}
