using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Exceptions;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace HuanyuFlowerShop.Services
{
    /// <summary>
    /// 商品服务实现类
    /// 提供商品相关的业务逻辑
    /// 对应前端商品列表、商品详情、商品搜索等功能
    /// </summary>
    public class ProductService(IUnitOfWork unitOfWork, IProductRepository productRepository, ICacheService cacheService, ApplicationDbContext context, ILogger<ProductService> logger) : IProductService
    {
        /// <summary>
        /// 工作单元接口
        /// 用于管理事务
        /// </summary>
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        
        /// <summary>
        /// 商品仓储接口
        /// 用于商品数据的访问
        /// </summary>
        private readonly IProductRepository _productRepository = productRepository;
        
        /// <summary>
        /// 缓存服务接口
        /// 用于缓存商品数据
        /// </summary>
        private readonly ICacheService _cacheService = cacheService;
        
        /// <summary>
        /// 数据库上下文
        /// 用于直接访问数据库
        /// </summary>
        private readonly ApplicationDbContext _context = context;
        
        /// <summary>
        /// 日志记录器
        /// 用于记录商品服务的日志
        /// </summary>
        private readonly ILogger<ProductService> _logger = logger;
        
        /// <summary>
        /// 缓存前缀
        /// 用于区分不同类型的缓存
        /// </summary>
        private const string CACHE_PREFIX = "product_";
        
        /// <summary>
        /// 所有商品缓存键
        /// 用于缓存所有商品列表
        /// </summary>
        private const string ALL_PRODUCTS_CACHE_KEY = "all_products";
        
        /// <summary>
        /// 推荐商品缓存键
        /// 用于缓存推荐商品列表
        /// </summary>
        private const string FEATURED_PRODUCTS_CACHE_KEY = "featured_products";
        
        /// <summary>
        /// 首页商品缓存键
        /// 用于缓存首页商品列表
        /// </summary>
        private const string HOME_PRODUCTS_CACHE_KEY = "home_products";

        

        /// <summary>
        /// 获取所有商品列表
        /// 对应API: GET api/Products
        /// </summary>
        /// <returns>商品列表</returns>
        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            try
            {
                var cacheKey = ALL_PRODUCTS_CACHE_KEY;
                var cachedProducts = await _cacheService.GetAsync<IEnumerable<ProductDto>>(cacheKey);
                
                if (cachedProducts != null)
                {
                    _logger.LogInformation("从缓存获取所有商品");
                    return cachedProducts;
                }

                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var soldCounts = await _context.OrderItems
                    .Where(oi => _context.Orders.Where(o => o.Status == "delivered" || o.Status == "shipped").Select(o => o.Id).Contains(oi.OrderId))
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                    .ToDictionaryAsync(x => x.ProductId, x => x.Qty);

                var popularityCounts = await _context.Favorites
                    .GroupBy(f => f.ProductId)
                    .Select(g => new { ProductId = g.Key, Cnt = g.Count() })
                    .ToDictionaryAsync(x => x.ProductId, x => x.Cnt);

                // 搜索结果的受欢迎度按收藏数量统计
                var searchPopularity = await _context.Favorites
                    .Where(f => products.Select(p => p.Id).Contains(f.ProductId))
                    .GroupBy(f => f.ProductId)
                    .Select(g => new { ProductId = g.Key, Cnt = g.Count() })
                    .ToDictionaryAsync(x => x.ProductId, x => x.Cnt);

                var ratingStats = await _context.ProductReviews
                    .Where(r => !r.IsDeleted && products.Select(p => p.Id).Contains(r.ProductId))
                    .GroupBy(r => r.ProductId)
                    .Select(g => new { ProductId = g.Key, Cnt = g.Count(), Avg = g.Average(x => x.Rating) })
                    .ToDictionaryAsync(x => x.ProductId, x => new { x.Cnt, x.Avg });

                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description ?? string.Empty,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl ?? string.Empty,
                    Stock = p.Stock,
                    IsFeatured = p.IsFeatured,
                    IsActive = p.IsActive,
                    SalesCount = soldCounts.TryGetValue(p.Id, out var sc1) && sc1 > 0 ? sc1 : p.SalesCount,
                    Popularity = popularityCounts.TryGetValue(p.Id, out var pop1) ? pop1 : 0,
                    AverageRating = ratingStats.TryGetValue(p.Id, out var rs1) ? Math.Round(rs1.Avg, 1) : 0,
                    ReviewCount = ratingStats.TryGetValue(p.Id, out var rs1b) ? rs1b.Cnt : 0,
                    Size = p.Size ?? string.Empty,
                    Material = p.Material ?? string.Empty,
                    Occasion = p.Occasion ?? string.Empty,
                    CategoryId = p.CategoryId,
                    Category = p.Category != null ? new CategoryDto 
                    { 
                        Id = p.Category.Id, 
                        Name = p.Category.Name ?? string.Empty, 
                        Description = p.Category.Description ?? string.Empty 
                    } : null,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                await _cacheService.SetAsync(cacheKey, productDtos, TimeSpan.FromSeconds(30));
                
                _logger.LogInformation("获取所有商品成功，数量: {Count}", productDtos.Count);
                return productDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取所有商品时发生错误");
                throw new BusinessException("获取商品列表失败", ex);
            }
        }

        /// <summary>
        /// 根据ID获取商品详情
        /// 对应API: GET api/Products/{id}
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns>商品详情</returns>
        public async Task<ProductDto?> GetProductByIdAsync(int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    _logger.LogWarning("无效的商品ID: {ProductId}", productId);
                    throw new ArgumentException("商品ID必须大于0", nameof(productId));
                }

                var cacheKey = $"{CACHE_PREFIX}{productId}";
                var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey);
                
                if (cachedProduct != null)
                {
                    _logger.LogInformation("从缓存获取商品，ID: {ProductId}", productId);
                    return cachedProduct;
                }

                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);
                    
                if (product == null)
                {
                    _logger.LogWarning("商品不存在，ID: {ProductId}", productId);
                    throw new ProductNotFoundException(productId);
                }

                var deliveredQty = await _context.OrderItems
                    .Where(oi => oi.ProductId == productId && _context.Orders.Where(o => o.Status == "delivered" || o.Status == "shipped").Select(o => o.Id).Contains(oi.OrderId))
                    .SumAsync(oi => (int?)oi.Quantity) ?? 0;

                var favoriteCnt = await _context.Favorites.CountAsync(f => f.ProductId == productId);

                var ratingStats1 = await _context.ProductReviews
                    .Where(r => !r.IsDeleted && r.ProductId == productId)
                    .GroupBy(r => r.ProductId)
                    .Select(g => new { ProductId = g.Key, Cnt = g.Count(), Avg = g.Average(x => x.Rating) })
                    .FirstOrDefaultAsync();

                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description ?? string.Empty,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl ?? string.Empty,
                    Stock = product.Stock,
                    IsFeatured = product.IsFeatured,
                    IsActive = product.IsActive,
                    SalesCount = deliveredQty > 0 ? deliveredQty : product.SalesCount,
                    Popularity = favoriteCnt,
                    AverageRating = ratingStats1 != null ? Math.Round(ratingStats1.Avg, 1) : 0,
                    ReviewCount = ratingStats1?.Cnt ?? 0,
                    Size = product.Size ?? string.Empty,
                    Material = product.Material ?? string.Empty,
                    Occasion = product.Occasion ?? string.Empty,
                    CategoryId = product.CategoryId,
                    Category = product.Category != null ? new CategoryDto 
                    { 
                        Id = product.Category.Id, 
                        Name = product.Category.Name ?? string.Empty, 
                        Description = product.Category.Description ?? string.Empty 
                    } : null,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };

                await _cacheService.SetAsync(cacheKey, productDto, TimeSpan.FromSeconds(30));
                
                _logger.LogInformation("获取商品成功，ID: {ProductId}", productId);
                return productDto;
            }
            catch (ProductNotFoundException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取商品时发生错误，商品ID: {ProductId}", productId);
                throw new BusinessException($"获取商品失败，商品ID: {productId}", ex);
            }
        }

        /// <summary>
        /// 根据分类获取商品列表
        /// 对应API: GET api/Products/category/{categoryId}
        /// </summary>
        /// <param name="categoryId">分类ID</param>
        /// <returns>商品列表</returns>
        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                {
                    _logger.LogWarning("无效的分类ID: {CategoryId}", categoryId);
                    throw new ArgumentException("分类ID必须大于0", nameof(categoryId));
                }

                var cacheKey = $"products_category_{categoryId}";
                var cachedProducts = await _cacheService.GetAsync<IEnumerable<ProductDto>>(cacheKey);
                
                if (cachedProducts != null)
                {
                    _logger.LogInformation("从缓存获取分类商品，分类ID: {CategoryId}", categoryId);
                    return cachedProducts;
                }

                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
                if (!categoryExists)
                {
                    _logger.LogWarning("分类不存在，分类ID: {CategoryId}", categoryId);
                    throw new CategoryNotFoundException(categoryId);
                }

                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == categoryId && p.IsActive)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
                var soldCounts2 = await _context.OrderItems
                    .Where(oi => _context.Orders.Where(o => o.Status == "delivered" || o.Status == "shipped").Select(o => o.Id).Contains(oi.OrderId))
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                    .ToDictionaryAsync(x => x.ProductId, x => x.Qty);

                var popularityCounts2 = await _context.Favorites
                    .GroupBy(f => f.ProductId)
                    .Select(g => new { ProductId = g.Key, Cnt = g.Count() })
                    .ToDictionaryAsync(x => x.ProductId, x => x.Cnt);

                var ratingStats2 = await _context.ProductReviews
                    .Where(r => !r.IsDeleted && products.Select(p => p.Id).Contains(r.ProductId))
                    .GroupBy(r => r.ProductId)
                    .Select(g => new { ProductId = g.Key, Cnt = g.Count(), Avg = g.Average(x => x.Rating) })
                    .ToDictionaryAsync(x => x.ProductId, x => new { x.Cnt, x.Avg });

                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description ?? string.Empty,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl ?? string.Empty,
                    Stock = p.Stock,
                    IsFeatured = p.IsFeatured,
                    IsActive = p.IsActive,
                    SalesCount = soldCounts2.TryGetValue(p.Id, out var sc2) && sc2 > 0 ? sc2 : p.SalesCount,
                    Popularity = popularityCounts2.TryGetValue(p.Id, out var pop2) ? pop2 : 0,
                    AverageRating = ratingStats2.TryGetValue(p.Id, out var rs2) ? Math.Round(rs2.Avg, 1) : 0,
                    ReviewCount = ratingStats2.TryGetValue(p.Id, out var rs2b) ? rs2b.Cnt : 0,
                    Size = p.Size ?? string.Empty,
                    Material = p.Material ?? string.Empty,
                    Occasion = p.Occasion ?? string.Empty,
                    CategoryId = p.CategoryId,
                    Category = p.Category != null ? new CategoryDto 
                    { 
                        Id = p.Category.Id, 
                        Name = p.Category.Name ?? string.Empty, 
                        Description = p.Category.Description ?? string.Empty 
                    } : null,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                await _cacheService.SetAsync(cacheKey, productDtos, TimeSpan.FromSeconds(30));
                
                _logger.LogInformation("获取分类商品成功，分类ID: {CategoryId}, 数量: {Count}", categoryId, productDtos.Count);
                return productDtos;
            }
            catch (CategoryNotFoundException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分类商品时发生错误，分类ID: {CategoryId}", categoryId);
                throw new BusinessException($"获取分类商品失败，分类ID: {categoryId}", ex);
            }
        }

        /// <summary>
        /// 获取推荐商品列表
        /// 对应API: GET api/Products/featured
        /// </summary>
        /// <returns>推荐商品列表</returns>
        public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync()
        {
            try
            {
                var cacheKey = FEATURED_PRODUCTS_CACHE_KEY;
                var cachedProducts = await _cacheService.GetAsync<IEnumerable<ProductDto>>(cacheKey);
                
                if (cachedProducts != null)
                {
                    _logger.LogInformation("从缓存获取推荐商品");
                    return cachedProducts;
                }

                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsFeatured && p.IsActive)
                    .OrderByDescending(p => p.SalesCount)
                    .Take(8)
                    .ToListAsync();

                var popularityCounts3 = await _context.Favorites
                    .GroupBy(f => f.ProductId)
                    .Select(g => new { ProductId = g.Key, Cnt = g.Count() })
                    .ToDictionaryAsync(x => x.ProductId, x => x.Cnt);

                var ratingStats3 = await _context.ProductReviews
                    .Where(r => !r.IsDeleted && products.Select(p => p.Id).Contains(r.ProductId))
                    .GroupBy(r => r.ProductId)
                    .Select(g => new { ProductId = g.Key, Cnt = g.Count(), Avg = g.Average(x => x.Rating) })
                    .ToDictionaryAsync(x => x.ProductId, x => new { x.Cnt, x.Avg });

                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description ?? string.Empty,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl ?? string.Empty,
                    Stock = p.Stock,
                    IsFeatured = p.IsFeatured,
                    IsActive = p.IsActive,
                    SalesCount = p.SalesCount,
                    Popularity = popularityCounts3.TryGetValue(p.Id, out var pop3) ? pop3 : 0,
                    AverageRating = ratingStats3.TryGetValue(p.Id, out var rs3) ? Math.Round(rs3.Avg, 1) : 0,
                    ReviewCount = ratingStats3.TryGetValue(p.Id, out var rs3b) ? rs3b.Cnt : 0,
                    Size = p.Size ?? string.Empty,
                    Material = p.Material ?? string.Empty,
                    Occasion = p.Occasion ?? string.Empty,
                    CategoryId = p.CategoryId,
                    Category = p.Category != null ? new CategoryDto 
                    { 
                        Id = p.Category.Id, 
                        Name = p.Category.Name ?? string.Empty, 
                        Description = p.Category.Description ?? string.Empty 
                    } : null,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                await _cacheService.SetAsync(cacheKey, productDtos, TimeSpan.FromSeconds(30));
                
                _logger.LogInformation("获取推荐商品成功，数量: {Count}", productDtos.Count);
                return productDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取推荐商品时发生错误");
                throw new BusinessException("获取推荐商品失败", ex);
            }
        }
        
        /// <summary>
        /// 搜索商品
        /// 对应API: GET api/Products/search
        /// </summary>
        /// <param name="searchDto">搜索条件</param>
        /// <returns>分页搜索结果</returns>
        public async Task<HuanyuFlowerShop.DTOs.PagedResult<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto)
        {
            try
            {
                // 验证分页参数
                if (searchDto.Page < 1) searchDto.Page = 1;
                if (searchDto.PageSize < 1 || searchDto.PageSize > 100) searchDto.PageSize = 12;
                
                // 构建查询
                var query = _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive);
                
                // 应用搜索条件
                if (!string.IsNullOrEmpty(searchDto.Keyword))
                {
                    var keyword = $"%{searchDto.Keyword}%";
                    query = query.Where(p => 
                        (!string.IsNullOrEmpty(p.Name) && EF.Functions.Like(p.Name, keyword)) ||
                        (!string.IsNullOrEmpty(p.Description) && EF.Functions.Like(p.Description, keyword)) ||
                        (!string.IsNullOrEmpty(p.Size) && EF.Functions.Like(p.Size, keyword)) ||
                        (!string.IsNullOrEmpty(p.Material) && EF.Functions.Like(p.Material, keyword)) ||
                        (!string.IsNullOrEmpty(p.Occasion) && EF.Functions.Like(p.Occasion, keyword)));
                }
                
                // 分类过滤
                if (searchDto.CategoryId.HasValue && searchDto.CategoryId > 0)
                {
                    query = query.Where(p => p.CategoryId == searchDto.CategoryId.Value);
                }
                
                // 价格范围过滤
                if (searchDto.MinPrice.HasValue && searchDto.MinPrice > 0)
                {
                    query = query.Where(p => p.Price >= searchDto.MinPrice.Value);
                }
                if (searchDto.MaxPrice.HasValue && searchDto.MaxPrice > 0)
                {
                    query = query.Where(p => p.Price <= searchDto.MaxPrice.Value);
                }
                
                // 尺寸过滤
                if (!string.IsNullOrEmpty(searchDto.Size))
                {
                    query = query.Where(p => p.Size == searchDto.Size);
                }
                
                // 材质过滤
                if (!string.IsNullOrEmpty(searchDto.Material))
                {
                    query = query.Where(p => p.Material == searchDto.Material);
                }
                
                // 场合过滤
                if (!string.IsNullOrEmpty(searchDto.Occasion))
                {
                    query = query.Where(p => p.Occasion == searchDto.Occasion);
                }
                
                // 库存过滤
                if (searchDto.InStockOnly.HasValue && searchDto.InStockOnly.Value)
                {
                    query = query.Where(p => p.Stock > 0);
                }
                
                // 推荐商品过滤
                if (searchDto.FeaturedOnly.HasValue && searchDto.FeaturedOnly.Value)
                {
                    query = query.Where(p => p.IsFeatured);
                }
                
                // 排序
                if (string.Equals(searchDto.SortBy, "price_asc", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.OrderBy(p => p.Price);
                }
                else if (string.Equals(searchDto.SortBy, "price_desc", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.OrderByDescending(p => p.Price);
                }
                else if (string.Equals(searchDto.SortBy, "sales", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.OrderByDescending(p => p.SalesCount);
                }
                else
                {
                    query = query.OrderByDescending(p => p.CreatedAt); // 默认按最新排序
                }
                
                // 获取总记录数
                var totalCount = await query.CountAsync();
                
                // 分页
                var products = await query
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();
                
                // 受欢迎度（收藏数）
                var searchPopularity = await _context.Favorites
                    .Where(f => products.Select(p => p.Id).Contains(f.ProductId))
                    .GroupBy(f => f.ProductId)
                    .Select(g => new { ProductId = g.Key, Cnt = g.Count() })
                    .ToDictionaryAsync(x => x.ProductId, x => x.Cnt);

                // 转换为DTO
                var ratingStats4 = await _context.ProductReviews
                    .Where(r => !r.IsDeleted && products.Select(p => p.Id).Contains(r.ProductId))
                    .GroupBy(r => r.ProductId)
                    .Select(g => new { ProductId = g.Key, Cnt = g.Count(), Avg = g.Average(x => x.Rating) })
                    .ToDictionaryAsync(x => x.ProductId, x => new { x.Cnt, x.Avg });

                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description ?? string.Empty,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl ?? string.Empty,
                    Stock = p.Stock,
                    IsFeatured = p.IsFeatured,
                    IsActive = p.IsActive,
                    SalesCount = p.SalesCount,
                    Popularity = searchPopularity.TryGetValue(p.Id, out var pop) ? pop : 0,
                    AverageRating = ratingStats4.TryGetValue(p.Id, out var rs4) ? Math.Round(rs4.Avg, 1) : 0,
                    ReviewCount = ratingStats4.TryGetValue(p.Id, out var rs4b) ? rs4b.Cnt : 0,
                    Size = p.Size ?? string.Empty,
                    Material = p.Material ?? string.Empty,
                    Occasion = p.Occasion ?? string.Empty,
                    CategoryId = p.CategoryId,
                    Category = p.Category != null ? new CategoryDto 
                    { 
                        Id = p.Category.Id, 
                        Name = p.Category.Name ?? string.Empty, 
                        Description = p.Category.Description ?? string.Empty 
                    } : null,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();
                
                // 返回分页结果
                var result = new HuanyuFlowerShop.DTOs.PagedResult<ProductDto>
                {
                    Items = productDtos,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize
                };
                
                _logger.LogInformation("搜索商品成功，关键词: {Keyword}, 分类: {CategoryId}, 数量: {Count}, 页码: {Page}", 
                    searchDto.Keyword, searchDto.CategoryId, productDtos.Count, searchDto.Page);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜索商品时发生错误，关键词: {Keyword}", searchDto.Keyword);
                throw new BusinessException("搜索商品失败", ex);
            }
        }

        /// <summary>
        /// 获取首页商品列表
        /// 对应API: GET api/Products/home
        /// </summary>
        /// <returns>首页商品列表</returns>
        public async Task<IEnumerable<ProductDto>> GetHomeProductsAsync()
        {
            try
            {
                var cacheKey = HOME_PRODUCTS_CACHE_KEY;
                var cachedProducts = await _cacheService.GetAsync<IEnumerable<ProductDto>>(cacheKey);
                
                if (cachedProducts != null)
                {
                    _logger.LogInformation("从缓存获取首页商品");
                    return cachedProducts;
                }

                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && p.Stock > 0)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(4)
                    .ToListAsync();
                var soldCounts3 = await _context.OrderItems
                    .Where(oi => _context.Orders.Where(o => o.Status == "delivered" || o.Status == "shipped").Select(o => o.Id).Contains(oi.OrderId))
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                    .ToDictionaryAsync(x => x.ProductId, x => x.Qty);

                var popularityCounts4 = await _context.Favorites
                    .GroupBy(f => f.ProductId)
                    .Select(g => new { ProductId = g.Key, Cnt = g.Count() })
                    .ToDictionaryAsync(x => x.ProductId, x => x.Cnt);

                var ratingStats5 = await _context.ProductReviews
                    .Where(r => !r.IsDeleted && products.Select(p => p.Id).Contains(r.ProductId))
                    .GroupBy(r => r.ProductId)
                    .Select(g => new { ProductId = g.Key, Cnt = g.Count(), Avg = g.Average(x => x.Rating) })
                    .ToDictionaryAsync(x => x.ProductId, x => new { x.Cnt, x.Avg });

                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description ?? string.Empty,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl ?? string.Empty,
                    Stock = p.Stock,
                    IsFeatured = p.IsFeatured,
                    IsActive = p.IsActive,
                    SalesCount = soldCounts3.TryGetValue(p.Id, out var sc3) && sc3 > 0 ? sc3 : p.SalesCount,
                    Popularity = popularityCounts4.TryGetValue(p.Id, out var pop4) ? pop4 : 0,
                    AverageRating = ratingStats5.TryGetValue(p.Id, out var rs5) ? Math.Round(rs5.Avg, 1) : 0,
                    ReviewCount = ratingStats5.TryGetValue(p.Id, out var rs5b) ? rs5b.Cnt : 0,
                    Size = p.Size ?? string.Empty,
                    Material = p.Material ?? string.Empty,
                    Occasion = p.Occasion ?? string.Empty,
                    CategoryId = p.CategoryId,
                    Category = p.Category != null ? new CategoryDto 
                    { 
                        Id = p.Category.Id, 
                        Name = p.Category.Name, 
                        Description = p.Category.Description 
                    } : null,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                await _cacheService.SetAsync(cacheKey, productDtos, TimeSpan.FromSeconds(30));
                
                _logger.LogInformation("获取首页商品成功，数量: {Count}", productDtos.Count);
                return productDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取首页商品时发生错误");
                throw new BusinessException("获取首页商品失败", ex);
            }
        }

        /// <summary>
        /// 创建商品
        /// 对应API: POST api/Products
        /// </summary>
        /// <param name="createProductDto">创建商品请求</param>
        /// <returns>创建的商品</returns>
        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            try
            {
                if (createProductDto == null)
                {
                    throw new ArgumentNullException(nameof(createProductDto), "创建商品数据不能为空");
                }

                if (string.IsNullOrWhiteSpace(createProductDto.Name))
                {
                    throw new ArgumentException("商品名称不能为空", nameof(createProductDto));
                }

                if (createProductDto.Price <= 0)
                {
                    throw new ArgumentException("商品价格必须大于0", nameof(createProductDto));
                }

                if (!createProductDto.CategoryId.HasValue)
                {
                    throw new ArgumentException("商品分类不能为空", nameof(createProductDto));
                }

                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == createProductDto.CategoryId.Value);
                if (!categoryExists)
                {
                    _logger.LogWarning("分类不存在，分类ID: {CategoryId}", createProductDto.CategoryId!.Value);
                    throw new CategoryNotFoundException(createProductDto.CategoryId!.Value);
                }

                await _unitOfWork.BeginTransactionAsync();

                var product = new Product
                {
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    Price = createProductDto.Price,
                    ImageUrl = createProductDto.ImageUrl,
                    Stock = createProductDto.Stock,
                    IsFeatured = createProductDto.IsFeatured,
                    IsActive = createProductDto.IsActive,
                    Size = createProductDto.Size,
                    Material = createProductDto.Material,
                    Occasion = createProductDto.Occasion,
                    CategoryId = createProductDto.CategoryId.Value,
                    CreatedAt = DateTime.UtcNow,
                    SalesCount = 0,
                    Popularity = 0
                };
                
                _context.Products.Add(product);
                await _unitOfWork.SaveChangesAsync();

                await _context.Entry(product).Reference(p => p.Category).LoadAsync();
                
                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description ?? string.Empty,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl ?? string.Empty,
                    Stock = product.Stock,
                    IsFeatured = product.IsFeatured,
                    IsActive = product.IsActive,
                    SalesCount = product.SalesCount,
                    Popularity = await _context.Favorites.CountAsync(f => f.ProductId == product.Id),
                    Size = product.Size ?? string.Empty,
                    Material = product.Material ?? string.Empty,
                    Occasion = product.Occasion ?? string.Empty,
                    CategoryId = product.CategoryId,
                    Category = product.Category != null ? new CategoryDto 
                    { 
                        Id = product.Category.Id, 
                        Name = product.Category.Name ?? string.Empty, 
                        Description = product.Category.Description ?? string.Empty 
                    } : null,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };

                await _unitOfWork.CommitTransactionAsync();

                await ClearProductCaches();
                
                _logger.LogInformation("成功创建商品，ID: {ProductId}, 名称: {ProductName}", product.Id, product.Name);

                return productDto;
            }
            catch (CategoryNotFoundException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            catch (ArgumentException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "创建商品时发生错误");
                throw new BusinessException("创建商品失败", ex);
            }
        }
        
        /// <summary>
        /// 更新商品
        /// 对应API: PUT api/Products/{id}
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="updateProductDto">更新商品请求</param>
        /// <returns>更新后的商品</returns>
        public async Task<ProductDto?> UpdateProductAsync(int productId, UpdateProductDto updateProductDto)
        {
            try
            {
                if (productId <= 0)
                {
                    _logger.LogWarning("无效的商品ID: {ProductId}", productId);
                    throw new ArgumentException("商品ID必须大于0", nameof(productId));
                }

                if (updateProductDto == null)
                {
                    throw new ArgumentNullException(nameof(updateProductDto), "更新商品数据不能为空");
                }

                await _unitOfWork.BeginTransactionAsync();

                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == productId);
                    
                if (product == null)
                {
                    _logger.LogWarning("商品不存在，ID: {ProductId}", productId);
                    throw new ProductNotFoundException(productId);
                }
                
                if (updateProductDto.Name != null) product.Name = updateProductDto.Name;
                if (updateProductDto.Description != null) product.Description = updateProductDto.Description;
                if (updateProductDto.Price.HasValue) product.Price = updateProductDto.Price.Value;
                if (updateProductDto.ImageUrl != null) product.ImageUrl = updateProductDto.ImageUrl;
                if (updateProductDto.Stock.HasValue) product.Stock = updateProductDto.Stock.Value;
                if (updateProductDto.IsFeatured.HasValue) product.IsFeatured = updateProductDto.IsFeatured.Value;
                if (updateProductDto.IsActive.HasValue) product.IsActive = updateProductDto.IsActive.Value;
                if (updateProductDto.Popularity.HasValue) product.Popularity = updateProductDto.Popularity.Value;
                if (updateProductDto.Size != null) product.Size = updateProductDto.Size;
                if (updateProductDto.Material != null) product.Material = updateProductDto.Material;
                if (updateProductDto.Occasion != null) product.Occasion = updateProductDto.Occasion;
                if (updateProductDto.CategoryId.HasValue) {
                    var categoryExists = await _context.Categories.AnyAsync(c => c.Id == updateProductDto.CategoryId.Value);
                    if (!categoryExists)
                    {
                        _logger.LogWarning("分类不存在，分类ID: {CategoryId}", updateProductDto.CategoryId.Value);
                        throw new CategoryNotFoundException(updateProductDto.CategoryId.Value);
                    }
                    product.CategoryId = updateProductDto.CategoryId.Value;
                }
                
                product.UpdatedAt = DateTime.UtcNow;
                
                await _unitOfWork.SaveChangesAsync();
                
                if (updateProductDto.CategoryId.HasValue)
                {
                    await _context.Entry(product).Reference(p => p.Category).LoadAsync();
                }

                await _unitOfWork.CommitTransactionAsync();

                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description ?? string.Empty,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl ?? string.Empty,
                    Stock = product.Stock,
                    IsFeatured = product.IsFeatured,
                    IsActive = product.IsActive,
                    SalesCount = product.SalesCount,
                    Popularity = await _context.Favorites.CountAsync(f => f.ProductId == product.Id),
                    Size = product.Size ?? string.Empty,
                    Material = product.Material ?? string.Empty,
                    Occasion = product.Occasion ?? string.Empty,
                    CategoryId = product.CategoryId,
                    Category = product.Category != null ? new CategoryDto 
                    { 
                        Id = product.Category.Id, 
                        Name = product.Category.Name, 
                        Description = product.Category.Description 
                    } : null,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };

                await ClearProductCaches();
                await _cacheService.RemoveAsync($"{CACHE_PREFIX}{productId}");
                
                _logger.LogInformation("成功更新商品，ID: {ProductId}, 名称: {ProductName}", product.Id, product.Name);

                return productDto;
            }
            catch (ProductNotFoundException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            catch (CategoryNotFoundException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            catch (ArgumentException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "更新商品时发生错误，商品ID: {ProductId}", productId);
                throw new BusinessException($"更新商品失败，商品ID: {productId}", ex);
            }
        }
        
        /// <summary>
        /// 删除商品
        /// 对应API: DELETE api/Products/{id}
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <returns>删除结果</returns>
        public async Task<bool> DeleteProductAsync(int productId)
        {
            try
            {
                if (productId <= 0)
                {
                    throw new ArgumentException("商品ID必须大于0", nameof(productId));
                }

                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning("尝试删除不存在的商品，ID: {ProductId}", productId);
                    throw new ProductNotFoundException(productId);
                }

                await _unitOfWork.BeginTransactionAsync();

                var hasOrderItems = await _context.OrderItems.AnyAsync(oi => oi.ProductId == productId);
                if (hasOrderItems)
                {
                    product.IsActive = false;
                    product.UpdatedAt = DateTime.UtcNow;
                    _context.Products.Update(product);
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    _context.Products.Remove(product);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();

                await ClearProductCaches();
                await _cacheService.RemoveAsync($"{CACHE_PREFIX}{productId}");

                _logger.LogInformation("商品删除处理完成，ID: {ProductId}", productId);
                return true;
            }
            catch (ProductNotFoundException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            catch (ArgumentException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "删除商品时发生错误，商品ID: {ProductId}", productId);
                throw new BusinessException($"删除商品失败，商品ID: {productId}", ex);
            }
        }

        private async Task ClearProductCaches()
        {
            try { await _cacheService.RemoveAsync(ALL_PRODUCTS_CACHE_KEY); } catch {}
            try { await _cacheService.RemoveAsync(FEATURED_PRODUCTS_CACHE_KEY); } catch {}
            try { await _cacheService.RemoveAsync(HOME_PRODUCTS_CACHE_KEY); } catch {}
        }

    }
}
