using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Repositories;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.DTOs;
using AutoMapper;

namespace HuanyuFlowerShop.Controllers
{
    [Authorize(Roles = "admin")]
    [ApiController]
    [Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ICacheService _cacheService;
    private static readonly string[] AllowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public AdminController(ApplicationDbContext context, IWebHostEnvironment environment, ICacheService cacheService)
        {
            _context = context;
            _environment = environment;
            _cacheService = cacheService;
        }

    

        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var today = DateTime.Today;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);

                // 用户统计
                var totalUsers = await _context.Users.CountAsync();
                var newUsers = await _context.Users.CountAsync(u => u.CreatedAt >= startOfMonth);
                var activeUsers = await _context.Users.CountAsync(u => u.Status == "active");
                var vipUsers = await _context.Users.CountAsync(u => u.Role == "vip");

                // 订单统计
                var totalOrders = await _context.Orders.CountAsync();
                var pendingOrders = await _context.Orders
                    .CountAsync(o => o.Status == "pending");

                // 商品统计
                var totalProducts = await _context.Products.CountAsync();
                var lowStockProducts = await _context.Products
                    .CountAsync(p => p.Stock <= 10);

                // 收入统计
                var totalRevenue = await _context.Orders
                    .Where(o => o.Status != "cancelled")
                    .SumAsync(o => o.TotalAmount);
                
                var todayRevenue = await _context.Orders
                    .Where(o => o.CreatedAt >= today && o.Status != "cancelled")
                    .SumAsync(o => o.TotalAmount);

                return Ok(new
                {
                    totalUsers,
                    newUsers,
                    activeUsers,
                    vipUsers,
                    totalOrders,
                    pendingOrders,
                    totalProducts,
                    lowStockProducts,
                    totalRevenue,
                    todayRevenue
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取统计数据失败", error = ex.Message });
            }
        }

        [HttpGet("dashboard/sales")]
        public async Task<IActionResult> GetSalesStats([FromQuery] int days = 30, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            try
            {
                var startDate = from?.Date ?? DateTime.Today.AddDays(-days);
                var endDate = to?.Date ?? DateTime.Today;
                var sales = await _context.Orders
                    .Where(o => o.CreatedAt.Date >= startDate && o.CreatedAt.Date <= endDate && o.Status != "cancelled")
                    .GroupBy(o => o.CreatedAt.Date)
                    .Select(g => new
                    {
                        date = g.Key.ToString("yyyy-MM-dd"),
                        revenue = g.Sum(o => o.TotalAmount),
                        orders = g.Count()
                    })
                    .OrderBy(x => x.date)
                    .ToListAsync();

                return Ok(sales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取销售统计失败", error = ex.Message });
            }
        }

        [HttpGet("dashboard/product-ranking")]
        public async Task<IActionResult> GetProductSalesRanking([FromQuery] int limit = 10)
        {
            try
            {
                // 使用Include来获取商品的分类信息
                var query = from oi in _context.OrderItems
                            join o in _context.Orders on oi.OrderId equals o.Id
                            where o.Status != "cancelled"
                            join p in _context.Products on oi.ProductId equals p.Id
                            join c in _context.Categories on p.CategoryId equals c.Id into categories
                            from category in categories.DefaultIfEmpty()
                            group new { oi, p, category } by new
                            {
                                p.Id,
                                p.Name,
                                p.ImageUrl,
                                p.Price,
                                CategoryName = category != null ? category.Name : "未分类"
                            } into g
                            select new
                            {
                                id = g.Key.Id,
                                name = g.Key.Name,
                                image = g.Key.ImageUrl,
                                price = g.Key.Price,
                                category = g.Key.CategoryName,
                                salesCount = g.Sum(x => x.oi.Quantity),
                                totalRevenue = g.Sum(x => x.oi.Subtotal)
                            };

                var topProducts = await query
                    .OrderByDescending(x => x.salesCount)
                    .Take(limit)
                    .ToListAsync();

                return Ok(topProducts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取商品销售排行失败", error = ex.Message });
            }
        }

        [HttpGet("dashboard/user-growth")]
        public async Task<IActionResult> GetUserGrowthStats([FromQuery] int days = 30, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            try
            {
                var startDate = from?.Date ?? DateTime.Today.AddDays(-days);
                var endDate = to?.Date ?? DateTime.Today;
                var userGrowth = await _context.Users
                    .Where(u => u.CreatedAt.Date >= startDate && u.CreatedAt.Date <= endDate)
                    .GroupBy(u => u.CreatedAt.Date)
                    .Select(g => new
                    {
                        date = g.Key.ToString("yyyy-MM-dd"),
                        newUsers = g.Count()
                    })
                    .OrderBy(x => x.date)
                    .ToListAsync();

                return Ok(userGrowth);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取用户增长统计失败", error = ex.Message });
            }
        }

        [HttpPut("users/{id}/role")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                var role = (dto?.Role ?? string.Empty).ToLowerInvariant();
                var allowed = new[] { "admin", "user", "vip" };
                if (!allowed.Contains(role))
                {
                    return BadRequest(new { message = "角色无效" });
                }

                user.Role = role;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await _cacheService.RemoveAsync("featured_products");
                await _cacheService.RemoveAsync("home_products");
                return Ok(new { message = "角色已更新", id, role });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新角色失败", error = ex.Message });
            }
        }

        [HttpPost("users/{id}/reset-password")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ResetUserPassword(int id, [FromBody] AdminResetPasswordDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                var newPassword = string.IsNullOrWhiteSpace(dto?.NewPassword) ? "123456" : dto!.NewPassword!;
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await _cacheService.RemoveAsync("featured_products");
                await _cacheService.RemoveAsync("home_products");
                return Ok(new { message = "密码已重置", id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "重置密码失败", error = ex.Message });
            }
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetAdminProducts([
            FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] string sortOrder = "desc",
            [FromQuery] string? status = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Products.AsQueryable();
                
                // 根据状态筛选商品
                if (string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
                {
                    // 不筛选状态
                }
                else if (status == "active")
                {
                    query = query.Where(p => p.IsActive == true);
                }
                else if (status == "inactive")
                {
                    query = query.Where(p => p.IsActive == false);
                }
                // 如果未指定状态，默认只返回活跃商品
                else if (status == null)
                {
                    query = query.Where(p => p.IsActive == true);
                }

                // 分类筛选
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                // 关键词筛选（名称/描述）
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var like = $"%{search}%";
                    query = query.Where(p =>
                        EF.Functions.Like(p.Name, like) ||
                        (!string.IsNullOrEmpty(p.Description) && EF.Functions.Like(p.Description, like))
                    );
                }

                // 排序
                query = (sortBy ?? "createdAt").Equals("name", StringComparison.OrdinalIgnoreCase) ?
                    (string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase) ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)) :
                    (sortBy ?? "createdAt").Equals("price", StringComparison.OrdinalIgnoreCase) ?
                    (string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase) ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price)) :
                    (sortBy ?? "createdAt").Equals("stock", StringComparison.OrdinalIgnoreCase) ?
                    (string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase) ? query.OrderByDescending(p => p.Stock) : query.OrderBy(p => p.Stock)) :
                    (sortBy ?? "createdAt").Equals("salescount", StringComparison.OrdinalIgnoreCase) ?
                    (string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase) ? query.OrderByDescending(p => p.SalesCount) : query.OrderBy(p => p.SalesCount)) :
                    (string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase) ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt));

                var total = await query.CountAsync();
                var products = await query
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                return Ok(new
                {
                    data = products,
                    total,
                    page,
                    limit,
                    totalPages = (int)Math.Ceiling((double)total / limit)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取商品列表失败", error = ex.Message });
            }
        }

        [HttpPost("products")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                if (!request.CategoryId.HasValue || request.CategoryId.Value <= 0)
                {
                    return BadRequest(new { message = "分类不能为空", code = "CATEGORY_REQUIRED" });
                }
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId.Value);
                if (!categoryExists)
                {
                    return BadRequest(new { message = "分类不存在", code = "CATEGORY_NOT_FOUND" });
                }
                var product = new Product
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    Stock = request.Stock,
                    ImageUrl = request.ImageUrl ?? "/images/default-product.svg",
                    Size = request.Size,
                    Material = request.Material,
                    Occasion = request.Occasion,
                    IsFeatured = request.IsFeatured,
                    IsActive = request.IsActive,
                    CategoryId = request.CategoryId ?? 0,
                    SalesCount = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAdminProductById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "创建商品失败", error = ex.Message });
            }
        }

        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetAdminProductById(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "商品不存在" });
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取商品详情失败", error = ex.Message });
            }
        }

        [HttpPut("products/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "商品不存在" });
                }

                product.Name = request.Name ?? product.Name;
                product.Description = request.Description ?? product.Description;
                product.Price = request.Price ?? product.Price;
                product.Stock = request.Stock ?? product.Stock;
                product.ImageUrl = request.ImageUrl ?? product.ImageUrl;
                product.Size = request.Size ?? product.Size;
                product.Material = request.Material ?? product.Material;
                product.Occasion = request.Occasion ?? product.Occasion;
                product.IsFeatured = request.IsFeatured ?? product.IsFeatured;
                product.IsActive = request.IsActive ?? product.IsActive;
                if (request.CategoryId.HasValue)
                {
                    if (request.CategoryId.Value <= 0)
                    {
                        return BadRequest(new { message = "分类不能为空", code = "CATEGORY_REQUIRED" });
                    }
                    var categoryExists2 = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId.Value);
                    if (!categoryExists2)
                    {
                        return BadRequest(new { message = "分类不存在", code = "CATEGORY_NOT_FOUND" });
                    }
                    product.CategoryId = request.CategoryId.Value;
                }
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新商品失败", error = ex.Message });
            }
        }

        [HttpDelete("products/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.CartItems)
                    .Include(p => p.OrderItems)
                    .FirstOrDefaultAsync(p => p.Id == id);
                    
                if (product == null)
                {
                    return NotFound(new { message = "商品不存在" });
                }

                // 检查是否有相关的购物车项或订单项
                var hasCartItems = product.CartItems != null && product.CartItems.Count != 0;
                var hasOrderItems = product.OrderItems != null && product.OrderItems.Count != 0;
                if (hasCartItems || hasOrderItems)
                {
                    // 如果有相关记录，选择软删除（设置IsActive为false）
                    product.IsActive = false;
                    product.UpdatedAt = DateTime.UtcNow;
                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // 如果没有相关记录，可以直接删除
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                }
                
                // 清除商品相关缓存
                await _cacheService.RemoveAsync("all_products");
                await _cacheService.RemoveAsync("featured_products");
                await _cacheService.RemoveAsync("home_products");
                await _cacheService.RemoveAsync($"product_{id}");
                
                return Ok(new { message = hasCartItems || hasOrderItems ? "商品已下架（由于有关联订单或购物车项）" : "商品删除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "删除商品失败", error = ex.Message });
            }
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetAdminOrders([FromQuery] int page = 1, [FromQuery] int limit = 10,
            [FromQuery] string sortBy = "createdAt", [FromQuery] string sortOrder = "desc", [FromQuery] string status = null)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.User) // 包含用户信息
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(o => o.Status == status);
                }

                // 排序
                query = (sortBy ?? "createdAt").Equals("totalamount", StringComparison.OrdinalIgnoreCase) ?
                    (string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase) ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount)) :
                    (sortBy ?? "createdAt").Equals("status", StringComparison.OrdinalIgnoreCase) ?
                    (string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase) ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status)) :
                    (string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase) ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt));

                var total = await query.CountAsync();
                var orders = await query
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(o => new
                    {
                        o.UserId,
                        o.Id,
                        o.OrderNumber,
                        o.Status,
                        o.TotalAmount,
                        o.DeliveryAddress,
                        o.RecipientName, // 添加收货人姓名
                        o.RecipientPhone,
                        o.CreatedAt,
                        o.UpdatedAt,
                        // 添加用户信息，包括头像
                        user = new
                        {
                            id = o.User.Id,
                            name = o.User.Username,
                            avatar = o.User.Avatar,
                            phone = o.User.Phone
                        },
                        orderItems = o.OrderItems.Select(oi => new
                        {
                            oi.Id,
                            oi.ProductId,
                            oi.ProductName,
                            oi.ProductImage,
                            oi.UnitPrice,
                            oi.Quantity,
                            oi.Subtotal
                        }).ToList()
                    })
                    .ToListAsync();

                // 构建结果，直接使用订单的RecipientName作为customerName
                var result = orders.Select(o => new
                {
                    o.UserId,
                    o.Id,
                    o.OrderNumber,
                    o.Status,
                    o.TotalAmount,
                    o.DeliveryAddress,
                    o.RecipientPhone,
                    o.CreatedAt,
                    o.UpdatedAt,
                    o.RecipientName,
                    customerName = o.RecipientName, // 使用真实的收货人姓名
                    o.user,
                    o.orderItems
                }).ToList();

                return Ok(new
                {
                    data = result,
                    total,
                    page,
                    limit,
                    totalPages = (int)Math.Ceiling((double)total / limit)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取订单列表失败", error = ex.Message });
            }
        }

        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetAdminOrderById(int id)
        {
            try
            {
                // 先获取订单基本信息
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.User) // 包含用户信息
                    .Where(o => o.Id == id)
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    return NotFound(new { message = "订单不存在" });
                }

                // 构建完整的订单信息，包含收货人姓名和用户头像
                var result = new
                {
                    order.UserId,
                    order.Id,
                    order.OrderNumber,
                    order.Status,
                    order.TotalAmount,
                    order.DeliveryAddress,
                    order.RecipientName, // 添加收货人姓名
                    order.RecipientPhone,
                    order.CreatedAt,
                    order.UpdatedAt,
                    // 添加用户信息，包括头像
                    user = new
                    {
                        id = order.User.Id,
                        name = order.User.Username,
                        avatar = order.User.Avatar,
                        phone = order.User.Phone
                    },
                    orderItems = order.OrderItems.Select(oi => new
                    {
                        oi.Id,
                        oi.ProductId,
                        oi.ProductName,
                        oi.ProductImage,
                        oi.UnitPrice,
                        oi.Quantity,
                        oi.Subtotal
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取订单详情失败", error = ex.Message });
            }
        }

        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Status))
                {
                    return BadRequest(new { message = "状态不能为空" });
                }

                var allowedStatuses = new[] { "pending", "processing", "shipped", "delivered", "cancelled" };
                if (!allowedStatuses.Contains(request.Status))
                {
                    return BadRequest(new { message = "非法的订单状态" });
                }

                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = "订单不存在" });
                }

                var oldStatus = order.Status;
                order.Status = request.Status;
                order.UpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(request.Note))
                {
                    order.Message = request.Note;
                }

                if (request.Status == "shipped")
                {
                    order.ShippedAt = DateTime.UtcNow;
                }
                else if (request.Status == "delivered")
                {
                    order.DeliveredAt = DateTime.UtcNow;
                }

                var operatorName = User?.Identity?.Name ?? "admin";
                _context.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    OldStatus = oldStatus,
                    NewStatus = request.Status,
                    OperatorId = null,
                    OperatorName = operatorName,
                    Note = request.Note,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                try
                {
                    await _cacheService.RemoveAsync($"order_{id}_{order.UserId}");
                    await _cacheService.RemoveAsync($"order_history_{id}");
                    await _cacheService.RemoveAsync($"user_orders_{order.UserId}");
                    await _cacheService.RemoveAsync($"user_order_stats_{order.UserId}");
                }
                catch {}

                return Ok(new { message = "订单状态更新成功", order });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新订单状态失败", error = ex.Message });
            }
        }

        [HttpDelete("orders/{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);
                if (order == null)
                {
                    return NotFound(new { message = "订单不存在" });
                }

                var histories = await _context.OrderStatusHistories
                    .Where(h => h.OrderId == id)
                    .ToListAsync();

                if (histories.Count != 0)
                {
                    _context.OrderStatusHistories.RemoveRange(histories);
                }

                if (order.OrderItems != null && order.OrderItems.Count > 0)
                {
                    _context.OrderItems.RemoveRange(order.OrderItems);
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                try
                {
                    await _cacheService.RemoveAsync($"order_{id}_{order.UserId}");
                    await _cacheService.RemoveAsync($"order_history_{id}");
                    await _cacheService.RemoveAsync($"user_orders_{order.UserId}");
                    await _cacheService.RemoveAsync($"user_order_stats_{order.UserId}");
                }
                catch {}

                return Ok(new { message = "订单已删除", id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "删除订单失败", error = ex.Message });
            }
        }

        [HttpGet("orders/{id}/history/latest")]
        public async Task<IActionResult> GetLatestOrderStatusHistory(int id)
        {
            try
            {
                var latest = await _context.OrderStatusHistories
                    .Where(h => h.OrderId == id)
                    .OrderByDescending(h => h.CreatedAt)
                    .Select(h => new
                    {
                        orderId = h.OrderId,
                        oldStatus = h.OldStatus,
                        newStatus = h.NewStatus,
                        operatorName = h.OperatorName,
                        note = h.Note,
                        createdAt = h.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (latest == null)
                {
                    return NotFound(new { message = "暂无处理记录" });
                }

                return Ok(latest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取最新处理记录失败", error = ex.Message });
            }
        }

        // 管理员删除指定用户的地址
        [HttpDelete("users/{id}/addresses/{addressId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUserAddress(int id, int addressId)
        {
            try
            {
                var address = await _context.Addresses
                    .Where(a => a.Id == addressId && a.UserId == id)
                    .FirstOrDefaultAsync();

                if (address == null)
                {
                    return NotFound(new { message = "地址不存在" });
                }

                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "删除地址失败", error = ex.Message });
            }
        }

        [HttpGet("users")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAdminUsers(int page = 1, int pageSize = 10, string? search = null, string? role = null, string? status = null)
        {
            try
            {
                // 避免使用可能导致隐式连接的Include，采用更简单的查询方式
                var query = _context.Users.AsQueryable();

                // 搜索功能
                if (!string.IsNullOrEmpty(search))
                {
                    var like = $"%{search}%";
                    query = query.Where(u =>
                        EF.Functions.Like(u.Username, like) ||
                        EF.Functions.Like(u.Email, like) ||
                        (u.Phone != null && EF.Functions.Like(u.Phone, like)) ||
                        (u.FullName != null && EF.Functions.Like(u.FullName, like))
                    );
                }

                // 按角色筛选
                if (!string.IsNullOrEmpty(role))
                {
                    query = query.Where(u => u.Role == role);
                }

                // 按状态筛选
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(u => u.Status == status);
                }

                // 分页
                var totalCount = await query.CountAsync();
                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 转换为DTO，单独查询每个用户的订单统计信息
                var userDtos = new List<UserDto>();
                foreach (var user in users)
                {
                    // 单独查询用户的地址
                    var addresses = await _context.Addresses
                        .Where(a => a.UserId == user.Id)
                        .Select(a => new AddressDto
                        {
                            Id = a.Id,
                            UserId = a.UserId,
                            RecipientName = a.RecipientName,
                            PhoneNumber = a.PhoneNumber,
                            Province = a.Province,
                            City = a.City,
                            District = a.District,
                            DetailAddress = a.DetailAddress,
                            IsDefault = a.IsDefault
                        })
                        .ToListAsync();
                    
                    // 单独查询用户的订单统计信息
                    var orderStats = await _context.Orders
                        .Where(o => o.UserId == user.Id)
                        .Select(o => new
                        {
                            Count = 1,
                            IsCompleted = o.Status == "completed",
                            Amount = o.TotalAmount
                        })
                        .ToListAsync();
                    
                    var totalOrders = orderStats.Count;
                    var completedOrders = orderStats.Count(os => os.IsCompleted);
                    var totalSpent = orderStats.Sum(os => os.Amount);
                    
                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Phone = user.Phone,
                        FullName = user.FullName,
                        Gender = user.Gender,
                        Role = user.Role,
                        Status = user.Status,
                        Avatar = user.Avatar,
                        Address = user.Address,
                        Addresses = addresses,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt,
                        LastLoginAt = user.LastLoginAt,
                        EmailVerified = false, // User实体中暂未实现该属性
                        Points = 0, // 积分系统暂未实现，默认为0
                        TotalOrders = totalOrders,
                        CompletedOrders = completedOrders,
                        TotalSpent = totalSpent
                    });
                }

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取用户列表失败", error = ex.Message });
            }
        }

        [HttpGet("users/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAdminUserById(int id)
        {
            try
            {
                // 直接查询用户基本信息，不使用Include
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                // 单独查询用户的地址列表
                var addresses = await _context.Addresses
                    .Where(a => a.UserId == id)
                    .Select(a => new AddressDto
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        RecipientName = a.RecipientName,
                        PhoneNumber = a.PhoneNumber,
                        Province = a.Province,
                        City = a.City,
                        District = a.District,
                        DetailAddress = a.DetailAddress,
                        IsDefault = a.IsDefault
                    })
                    .ToListAsync();

                // 单独查询用户的订单统计信息
                var totalOrders = await _context.Orders.CountAsync(o => o.UserId == id);
                var completedOrders = await _context.Orders.CountAsync(o => o.UserId == id && o.Status == "completed");
                var totalSpent = await _context.Orders.Where(o => o.UserId == id).SumAsync(o => o.TotalAmount);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Phone = user.Phone,
                    FullName = user.FullName,
                    Gender = user.Gender,
                    Role = user.Role,
                    Status = user.Status,
                    Avatar = user.Avatar,
                    Address = user.Address,
                    Addresses = addresses,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    LastLoginAt = user.LastLoginAt,
                    EmailVerified = false, // User实体中暂未实现该属性
                    Points = 0, // 积分系统暂未实现，默认为0
                    TotalOrders = totalOrders,
                    CompletedOrders = completedOrders,
                    TotalSpent = totalSpent
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取用户详情失败", error = ex.Message });
            }
        }

        [HttpPut("users/{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                user.Status = request.Status;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "用户状态更新成功", user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新用户状态失败", error = ex.Message });
            }
        }

        

        [HttpDelete("users/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                // 使用事务确保数据一致性
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    // 查找用户及其相关数据
                    var user = await _context.Users
                        .Include(u => u.Orders).ThenInclude(o => o.OrderItems)
                        .Include(u => u.Addresses)
                        .Include(u => u.CartItems)
                        .FirstOrDefaultAsync(u => u.Id == id);

                    if (user == null)
                    {
                        return NotFound(new { message = "用户不存在" });
                    }

                    // 删除用户的订单明细（如果存在）
                    foreach (var order in user.Orders)
                    {
                        _context.OrderItems.RemoveRange(order.OrderItems);
                    }

                    // 删除用户的订单
                    _context.Orders.RemoveRange(user.Orders);

                    // 删除用户的地址
                    _context.Addresses.RemoveRange(user.Addresses);

                    // 删除用户的购物车商品
                    _context.CartItems.RemoveRange(user.CartItems);

                    // 删除用户
                    _context.Users.Remove(user);

                    // 提交事务
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }

                return Ok(new { message = "用户及相关订单删除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "删除用户失败", error = ex.Message });
            }
        }

        [HttpPost("upload-product-image")]
        public async Task<IActionResult> UploadProductImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "请选择要上传的图片文件" });
                }

                // 验证文件类型
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedImageExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = "只支持 JPG、PNG、GIF、WebP 格式的图片文件" });
                }

                // 验证文件大小 (5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "图片文件大小不能超过 5MB" });
                }

                // 创建上传目录
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 生成唯一文件名
                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 保存文件
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 返回文件URL
                var fileUrl = $"/uploads/products/{uniqueFileName}";
                var fullUrl = $"{Request.Scheme}://{Request.Host}{fileUrl}";
                
                return Ok(new { 
                    message = "图片上传成功", 
                    imageUrl = fullUrl 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "图片上传失败", error = ex.Message });
            }
        }

        private static string GetCustomerNameFromAddress(string? deliveryAddress)
        {
            if (string.IsNullOrEmpty(deliveryAddress))
            {
                return "未知客户";
            }
        
            var parts = deliveryAddress.Split(' ');
            if (parts.Length > 1)
            {
                return parts[0] ?? "未知客户";
            }
        
            return deliveryAddress ?? "未知客户";
        }
    }

    // 请求模型
    public class CreateProductRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public string? Size { get; set; }
        public string? Material { get; set; }
        public string? Occasion { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;
        public int? CategoryId { get; set; }
    }

    public class UpdateProductRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public string? ImageUrl { get; set; }
        public string? Size { get; set; }
        public string? Material { get; set; }
        public string? Occasion { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsActive { get; set; }
        public int? CategoryId { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class UpdateUserRoleRequest
    {
        public required string Role { get; set; }
    }

    public class UpdateUserStatusRequest
    {
        public required string Status { get; set; }
    }

    // 使用DTOs命名空间中的AddressDto

    // 用户DTO
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? FullName { get; set; }
        public string Role { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Avatar { get; set; }
        public string? Address { get; set; }
        public List<AddressDto>? Addresses { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool EmailVerified { get; set; }
        public int Points { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
