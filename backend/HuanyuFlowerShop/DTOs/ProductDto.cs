namespace HuanyuFlowerShop.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Stock { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }
        public int SalesCount { get; set; }
        public int Popularity { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Occasion { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public CategoryDto? Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Stock { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }
        public int Popularity { get; set; } = 0;
        public string Size { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Occasion { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
    }

    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public int? Stock { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsActive { get; set; }
        public int? Popularity { get; set; }
        public string? Size { get; set; }
        public string? Material { get; set; }
        public string? Occasion { get; set; }
        public int? CategoryId { get; set; }
    }
}
