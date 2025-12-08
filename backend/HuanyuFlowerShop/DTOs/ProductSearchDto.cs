namespace HuanyuFlowerShop.DTOs
{
    /// <summary>
    /// 商品搜索和过滤条件DTO
    /// </summary>
    public class ProductSearchDto
    {
        /// <summary>
        /// 搜索关键词
        /// </summary>
        public string? Keyword { get; set; }
        
        /// <summary>
        /// 分类ID
        /// </summary>
        public int? CategoryId { get; set; }
        
        /// <summary>
        /// 最小价格
        /// </summary>
        public decimal? MinPrice { get; set; }
        
        /// <summary>
        /// 最大价格
        /// </summary>
        public decimal? MaxPrice { get; set; }
        
        /// <summary>
        /// 尺寸筛选
        /// </summary>
        public string? Size { get; set; }
        
        /// <summary>
        /// 材质筛选
        /// </summary>
        public string? Material { get; set; }
        
        /// <summary>
        /// 场合筛选
        /// </summary>
        public string? Occasion { get; set; }
        
        /// <summary>
        /// 是否只显示有库存的商品
        /// </summary>
        public bool? InStockOnly { get; set; }
        
        /// <summary>
        /// 是否只显示推荐商品
        /// </summary>
        public bool? FeaturedOnly { get; set; }
        
        /// <summary>
        /// 排序方式: price_asc, price_desc, newest, sales
        /// </summary>
        public string? SortBy { get; set; } = "newest";
        
        /// <summary>
        /// 页码
        /// </summary>
        public int Page { get; set; } = 1;
        
        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; } = 12;
    }
    
    /// <summary>
    /// 分页结果DTO
    /// </summary>
    public class PagedResult<T>
    {
        /// <summary>
        /// 数据列表
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();
        
        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// 当前页码
        /// </summary>
        public int Page { get; set; }
        
        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        
        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPrevious => Page > 1;
        
        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNext => Page < TotalPages;
    }
}