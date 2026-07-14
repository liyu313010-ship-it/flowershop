using Xunit;
using Moq;
using HuanyuFlowerShop.Services;
using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using HuanyuFlowerShop.DTOs;

namespace HuanyuFlowerShop.Tests
{
    public class ProductReviewTests
    {
        [Fact]
        public async Task CreateReviewAsync_ShouldCreateReview_WhenValidData()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // 创建测试数据
            using (var context = new ApplicationDbContext(options))
            {
                // 添加测试产品
                var product = new Product
                {
                    Name = "测试产品",
                    Description = "测试产品描述",
                    Price = 100,
                    ImageUrl = "test.jpg",
                    IsActive = true,
                    IsFeatured = false,
                    CategoryId = 1
                };
                await context.Products.AddAsync(product);

                // 添加测试用户
                var user = new User
                {
                    Username = "testuser",
                    Email = "test@example.com",
                    Password = "password123",
                    Role = "user",
                    Status = "active"
                };
                await context.Users.AddAsync(user);

                await context.SaveChangesAsync();

                var order = new Order
                {
                    UserId = user.Id,
                    OrderNumber = "TEST-ORDER-1",
                    Status = "delivered",
                    TotalAmount = product.Price,
                    Subtotal = product.Price,
                    ShippingFee = 0,
                    RecipientName = "测试用户",
                    RecipientPhone = "13800138000",
                    DeliveryAddress = "测试地址",
                    PaymentMethod = "cod",
                    PaymentStatus = "paid"
                };
                await context.Orders.AddAsync(order);
                await context.SaveChangesAsync();
                await context.OrderItems.AddAsync(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImage = product.ImageUrl,
                    UnitPrice = product.Price,
                    Quantity = 1,
                    Subtotal = product.Price
                });
                await context.SaveChangesAsync();
            }

            // 重新创建上下文以确保数据已保存
            using (var context = new ApplicationDbContext(options))
            {
                var service = new ProductReviewService(context);

                var request = new CreateProductReviewRequest
                {
                    ProductId = 1,
                    Rating = 5,
                    Comment = "这是一个测试评价"
                };

                // Act
                var result = await service.CreateReviewAsync(1, request);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(1, result.ProductId);
                Assert.Equal("测试产品", result.ProductName);
                Assert.Equal(5, result.Rating);
                Assert.Equal("这是一个测试评价", result.Comment);
                Assert.Equal("testuser", result.UserName);

                // 验证数据是否已保存到数据库
                var review = await context.ProductReviews.FirstOrDefaultAsync(r => r.Id == result.Id);
                Assert.NotNull(review);
                Assert.Equal(1, review.ProductId);
                Assert.Equal(1, review.UserId);
                Assert.Equal(5, review.Rating);
            }
        }

        [Fact]
        public async Task CreateReviewAsync_ShouldThrowException_WhenProductNotExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase2")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ProductReviewService(context);

                var request = new CreateProductReviewRequest
                {
                    ProductId = 999, // 不存在的产品ID
                    Rating = 5,
                    Comment = "这是一个测试评价"
                };

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() => service.CreateReviewAsync(1, request));
            }
        }
    }
}
