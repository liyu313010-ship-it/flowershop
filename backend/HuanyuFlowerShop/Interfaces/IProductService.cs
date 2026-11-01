using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.DTOs;

namespace HuanyuFlowerShop.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(int productId);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync();
        Task<IEnumerable<ProductDto>> GetHomeProductsAsync();
        Task<HuanyuFlowerShop.DTOs.PagedResult<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto);
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductDto?> UpdateProductAsync(int productId, UpdateProductDto updateProductDto);
        Task<bool> DeleteProductAsync(int productId);
    }
}
