using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using HuanyuFlowerShop.Middleware;

namespace HuanyuFlowerShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            _logger.LogInformation("开始获取商品列表");
            try
            {
                var products = await _productService.GetAllProductsAsync();
                _logger.LogInformation("成功获取商品列表，数量: {Count}", products.Count());
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取商品列表失败，返回空列表以保障页面可用性");
                return Ok(Enumerable.Empty<ProductDto>());
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            _logger.LogInformation("开始获取商品详情，ID: {ProductId}", id);
            var product = await _productService.GetProductByIdAsync(id);
            _logger.LogInformation("成功获取商品详情，ID: {ProductId}", id);
            return Ok(product);
        }

        [HttpGet("featured")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetFeaturedProducts()
        {
            _logger.LogInformation("开始获取推荐商品列表");
            try
            {
                var products = await _productService.GetFeaturedProductsAsync();
                _logger.LogInformation("成功获取推荐商品列表，数量: {Count}", products.Count());
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取推荐商品失败，返回空列表以保障页面可用性");
                return Ok(Enumerable.Empty<ProductDto>());
            }
        }

        [HttpGet("home")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetHomeProducts()
        {
            _logger.LogInformation("开始获取首页商品列表");
            try
            {
                var products = await _productService.GetHomeProductsAsync();
                _logger.LogInformation("成功获取首页商品列表，数量: {Count}", products.Count());
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取首页商品失败，返回空列表以保障页面可用性");
                return Ok(Enumerable.Empty<ProductDto>());
            }
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
        {
            if (categoryId <= 0)
            {
                return BadRequest(new { error = "分类ID必须大于0", code = "INVALID_CATEGORY_ID" });
            }

            _logger.LogInformation("开始获取分类商品，分类ID: {CategoryId}", categoryId);
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            _logger.LogInformation("成功获取分类商品，分类ID: {CategoryId}，数量: {Count}", categoryId, products.Count());
            return Ok(products);
        }
        
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<ProductDto>>> SearchProducts([FromQuery] ProductSearchDto searchDto)
        {
            _logger.LogInformation("开始搜索商品，关键词: {Keyword}", searchDto.Keyword);
            
            try
            {
                var result = await _productService.SearchProductsAsync(searchDto);
                _logger.LogInformation("成功搜索商品，返回: {Count}条，总记录: {TotalCount}", 
                    result.Items.Count, result.TotalCount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜索商品失败，关键词: {Keyword}", searchDto.Keyword);
                return StatusCode(500, new { error = "搜索商品失败，请稍后再试", code = "SEARCH_ERROR" });
            }
        }

        [HttpPost]
        [Authorize]
        [AdminOnly]
        public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
        {
            if (createProductDto == null)
            {
                return BadRequest(new { error = "商品信息不能为空", code = "INVALID_INPUT" });
            }

            _logger.LogInformation("开始创建商品，分类ID: {CategoryId}", createProductDto.CategoryId);
            var product = await _productService.CreateProductAsync(createProductDto);
            _logger.LogInformation("成功创建商品，ID: {ProductId}", product.Id);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, UpdateProductDto updateProductDto)
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "商品ID必须大于0", code = "INVALID_PRODUCT_ID" });
            }

            _logger.LogInformation("开始更新商品，ID: {ProductId}", id);
            var product = await _productService.UpdateProductAsync(id, updateProductDto);
            _logger.LogInformation("成功更新商品，ID: {ProductId}", id);
            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "商品ID必须大于0", code = "INVALID_PRODUCT_ID" });
            }

            _logger.LogInformation("开始删除商品，ID: {ProductId}", id);
            await _productService.DeleteProductAsync(id);
            _logger.LogInformation("成功删除商品，ID: {ProductId}", id);
            return NoContent();
        }


    }
}
