namespace HuanyuFlowerShop.DTOs
{
    /// <summary>
    /// 收藏商品请求DTO
    /// </summary>
    public class AddFavoriteDto
    {
        public int ProductId { get; set; }
    }

    /// <summary>
    /// 收藏商品响应DTO
    /// </summary>
    public class FavoriteDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 收藏商品列表响应DTO
    /// </summary>
    public class FavoriteListResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<FavoriteDto> Favorites { get; set; } = Enumerable.Empty<FavoriteDto>();
        public int TotalCount { get; set; }
    }
}
