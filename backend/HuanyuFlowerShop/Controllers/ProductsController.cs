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
using System;

namespace HuanyuFlowerShop.Controllers
{
    /// <summary>
    /// 商品控制器
    /// 负责处理商品相关的API请求
    /// 对应前端商品列表、商品详情、商品搜索等功能
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController(IProductService productService, ILogger<ProductsController> logger) : ControllerBase
    {
        /// <summary>
        /// 商品服务接口
        /// 提供商品相关的业务逻辑
        /// </summary>
        private readonly IProductService _productService = productService;
        
        /// <summary>
        /// 日志记录器
        /// 用于记录API请求和响应日志
        /// </summary>
        private readonly ILogger<ProductsController> _logger = logger;

        

        /// <summary>
        /// 获取所有商品列表
        /// GET: api/Products
        /// </summary>
        /// <returns>商品列表</returns>
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

        /// <summary>
        /// 获取商品详情
        /// GET: api/Products/{id}
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <returns>商品详情</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            _logger.LogInformation("开始获取商品详情，ID: {ProductId}", id);
            var product = await _productService.GetProductByIdAsync(id);
            _logger.LogInformation("成功获取商品详情，ID: {ProductId}", id);
            return Ok(product);
        }

        /// <summary>
        /// 获取推荐商品列表
        /// GET: api/Products/featured
        /// </summary>
        /// <returns>推荐商品列表</returns>
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

        /// <summary>
        /// 获取首页商品列表
        /// GET: api/Products/home
        /// </summary>
        /// <returns>首页商品列表</returns>
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

        /// <summary>
        /// 根据分类获取商品列表
        /// GET: api/Products/category/{categoryId}
        /// </summary>
        /// <param name="categoryId">分类ID</param>
        /// <returns>分类商品列表</returns>
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
        
        /// <summary>
        /// 搜索商品
        /// GET: api/Products/search
        /// </summary>
        /// <param name="searchDto">搜索条件</param>
        /// <returns>分页搜索结果</returns>
        [HttpGet("search")]
        public async Task<ActionResult<HuanyuFlowerShop.DTOs.PagedResult<ProductDto>>> SearchProducts([FromQuery] ProductSearchDto searchDto)
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

        /// <summary>
        /// 创建商品
        /// POST: api/Products
        /// 需要管理员权限
        /// </summary>
        /// <param name="createProductDto">创建商品请求</param>
        /// <returns>创建的商品</returns>
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

        /// <summary>
        /// 更新商品
        /// PUT: api/Products/{id}
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <param name="updateProductDto">更新商品请求</param>
        /// <returns>更新后的商品</returns>
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

        /// <summary>
        /// 删除商品
        /// DELETE: api/Products/{id}
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <returns>删除结果</returns>
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
