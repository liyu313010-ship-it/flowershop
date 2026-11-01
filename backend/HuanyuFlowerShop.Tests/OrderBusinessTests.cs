using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using HuanyuFlowerShop.Controllers;
using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;

using HuanyuFlowerShop.Repositories;
using HuanyuFlowerShop.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

// Regression checks use an isolated in-memory database. MySQL transaction/concurrency checks live in MySqlOrderConsistencyTests.
public sealed class OrderBusinessTests
{
    private const int UserId = 9001;
    private const int ProductId = 9001;

    private sealed class Fixture : IDisposable
    {
        public readonly DbContextOptions<ApplicationDbContext> DbOptions;
        public readonly ApplicationDbContext Db;
        public readonly MemoryCache Memory = new(new MemoryCacheOptions());
        public readonly ICacheService Cache;
        public readonly OrderService Orders;

        public Fixture()
        {
            DbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            Db = new ApplicationDbContext(DbOptions);
            Cache = new MemoryCacheService(Memory, NullLogger<MemoryCacheService>.Instance);
            Orders = new OrderService(
                new Repository<Order>(Db), new Repository<OrderItem>(Db),
                new Repository<CartItem>(Db), new Repository<Product>(Db),
                new Repository<OrderStatusHistory>(Db), new Repository<Coupon>(Db),
                new Repository<UserCoupon>(Db), Cache, NullLogger<OrderService>.Instance,
                Mock.Of<IPaymentService>(), new ConfigurationBuilder().Build(), new UnitOfWork(Db), Db);
            Db.Users.Add(new User { Id = UserId, Username = "audit-user", Email = "audit@example.invalid", Password = "unused" });
            Db.Products.Add(new Product { Id = ProductId, Name = "Original flower", ImageUrl = "/original.webp", Price = 100m, Stock = 10, IsActive = true });
            Db.CartItems.Add(new CartItem { UserId = UserId, ProductId = ProductId, Quantity = 1, Price = 100m });
            Db.SaveChanges();
            Db.ChangeTracker.Clear();
        }

        public Task<OrderDto> Create(string? coupon = null) => Orders.CreateOrderAsync(UserId,
            new CreateOrderDto { RecipientName = "Audit", Phone = "13800000000", ShippingAddress = "Audit address", PaymentMethod = "cod", CouponCode = coupon });

        public AdminController Admin() => new(Db, Mock.Of<IWebHostEnvironment>(), Cache);

        public async Task ExpireAndRunCleanup(int orderId)
        {
            var order = await Db.Orders.FindAsync(orderId);
            order!.PaymentExpiresAt = DateTime.UtcNow.AddMinutes(-1);
            await Db.SaveChangesAsync();
            var services = new ServiceCollection().AddSingleton<ICacheService>(Cache).AddScoped(_ => new ApplicationDbContext(DbOptions));
            using var provider = services.BuildServiceProvider();
            using var worker = new ExpiredOrderCleanupService(provider.GetRequiredService<IServiceScopeFactory>(), NullLogger<ExpiredOrderCleanupService>.Instance);
            var cleanup = typeof(ExpiredOrderCleanupService).GetMethod("CleanupAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
            await (Task)cleanup.Invoke(worker, new object[] { CancellationToken.None })!;
            Db.ChangeTracker.Clear();
        }

        public void Dispose() { Db.Dispose(); Memory.Dispose(); }
    }

    [Fact]
    public async Task Control_NormalOrderUsesServerPriceAndReservesStock()
    {
        using var f = new Fixture();
        var created = await f.Create();
        Assert.Equal(115m, created.TotalAmount);
        Assert.Equal(9, (await f.Db.Products.FindAsync(ProductId))!.Stock);
        Assert.Empty(await f.Db.CartItems.ToListAsync());
    }

    [Fact]
    public async Task Control_CancellationRestoresStockAndSequentialRepeatIsRejected()
    {
        using var f = new Fixture();
        var created = await f.Create();
        Assert.True(await f.Orders.CancelOrderAsync(UserId, created.Id));
        Assert.False(await f.Orders.CancelOrderAsync(UserId, created.Id));
        Assert.Equal(10, (await f.Db.Products.FindAsync(ProductId))!.Stock);
    }

    [Fact]
    public async Task Control_OtherUserCannotReadOrCancelOrder()
    {
        using var f = new Fixture();
        var created = await f.Create();
        Assert.Null(await f.Orders.GetOrderByIdAsync(UserId + 1, created.Id));
        Assert.False(await f.Orders.CancelOrderAsync(UserId + 1, created.Id));
    }

    [Fact]
    public async Task CashOnDelivery_ShouldNotBeClosedByOnlinePaymentDeadline()
    {
        using var f = new Fixture();
        var created = await f.Create();
        await f.ExpireAndRunCleanup(created.Id);
        Assert.Equal("pending", (await f.Db.Orders.FindAsync(created.Id))!.Status);
    }

    [Fact]
    public async Task AutomaticClosure_ShouldInvalidateCachedOrder()
    {
        using var f = new Fixture();
        var created = await f.Create(); // Creates the order-detail cache.
        var online = (await f.Db.Orders.FindAsync(created.Id))!;
        online.PaymentMethod = "online";
        await f.Db.SaveChangesAsync();
        await f.ExpireAndRunCleanup(created.Id);
        var actual = (await f.Db.Orders.FindAsync(created.Id))!.Status;
        var cached = (await f.Orders.GetOrderByIdAsync(UserId, created.Id))!.Status;
        Assert.Equal(actual, cached);
    }

    [Fact]
    public async Task UserCancellation_ShouldAddHistory()
    {
        using var f = new Fixture();
        var created = await f.Create();
        Assert.True(await f.Orders.CancelOrderAsync(UserId, created.Id));
        Assert.Contains(await f.Db.OrderStatusHistories.ToListAsync(), h => h.OrderId == created.Id && h.NewStatus == "cancelled");
    }

    [Fact]
    public async Task CancelledUnfulfilledOrder_ShouldReleaseCouponReservation()
    {
        using var f = new Fixture();
        f.Db.Coupons.Add(new Coupon { Id = 9001, Code = "AUDIT10", Value = 10m, Status = "active" });
        await f.Db.SaveChangesAsync();
        f.Db.ChangeTracker.Clear();
        var created = await f.Create("AUDIT10");
        Assert.True(await f.Orders.CancelOrderAsync(UserId, created.Id));
        Assert.NotEqual("used", (await f.Db.UserCoupons.SingleAsync()).Status);
    }

    [Fact]
    public async Task OrderDetail_ShouldKeepPurchasedProductSnapshot()
    {
        using var f = new Fixture();
        var created = await f.Create();
        var item = await f.Db.OrderItems.SingleAsync();
        var product = (await f.Db.Products.FindAsync(ProductId))!;
        product.Name = "Renamed flower";
        product.ImageUrl = "/new.webp";
        await f.Db.SaveChangesAsync();
        await f.Cache.ClearAsync();
        var dto = (await f.Orders.GetOrderByIdAsync(UserId, created.Id))!;
        Assert.Equal(item.ProductName, dto.OrderItems.Single().ProductName);
    }

    [Fact]
    public async Task DeletingPendingOrder_IsRejectedAndRetainsReservation()
    {
        using var f = new Fixture();
        var created = await f.Create();
        Assert.IsType<BadRequestObjectResult>(await f.Admin().DeleteOrder(created.Id));
        Assert.NotNull(await f.Db.Orders.FindAsync(created.Id));
        Assert.Equal(9, (await f.Db.Products.FindAsync(ProductId))!.Stock);
    }

    [Fact]
    public async Task GenericStatusUpdate_ShouldNotShipWithoutShippingInformation()
    {
        using var f = new Fixture();
        var created = await f.Create();
        await f.Admin().UpdateOrderStatus(created.Id, new HuanyuFlowerShop.Controllers.UpdateOrderStatusRequest { Status = "processing" });
        Assert.IsType<BadRequestObjectResult>(await f.Admin().UpdateOrderStatus(created.Id, new HuanyuFlowerShop.Controllers.UpdateOrderStatusRequest { Status = "shipped" }));
        Assert.Equal("processing", (await f.Db.Orders.FindAsync(created.Id))!.Status);
    }

    [Fact]
    public async Task CartResponse_ShouldExposeStockNeededByCartPage()
    {
        using var f = new Fixture();
        var carts = new CartService(new Repository<CartItem>(f.Db), new Repository<Product>(f.Db));
        var dto = (await carts.GetCartItemsAsync(UserId)).Single();
        Assert.NotNull(dto.GetType().GetProperty("Stock"));
    }

    [Fact]
    public async Task UsedCouponPreview_ShouldNotOfferDiscountAgain()
    {
        using var f = new Fixture();
        f.Db.Coupons.Add(new Coupon { Id = 9001, Code = "AUDIT10", Value = 10m, Status = "active", UsageLimit = 1, UsedCount = 1 });
        f.Db.UserCoupons.Add(new UserCoupon { UserId = UserId, CouponId = 9001, Status = "used" });
        await f.Db.SaveChangesAsync();
        var controller = new CouponsController(f.Db)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, UserId.ToString()) }, "test")) } }
        };
        var response = controller.Validate(new CouponValidateRequest { Code = "AUDIT10", OrderAmount = 100m });
        Assert.IsNotType<OkObjectResult>(response);
    }

    [Fact]
    public async Task InvalidCouponAtCheckout_ShouldNotSilentlyCreateFullPriceOrder()
    {
        using var f = new Fixture();
        await Assert.ThrowsAsync<ArgumentException>(() => f.Create("DOES_NOT_EXIST"));
    }

    [Theory]
    [InlineData("disable")]
    [InlineData("demote")]
    public async Task SecurityChange_ShouldRevokePreviouslyIssuedTokenVersion(string operation)
    {
        using var f = new Fixture();
        var user = (await f.Db.Users.FindAsync(UserId))!;
        user.Role = "admin";
        var priorVersion = user.TokenVersion;
        await f.Db.SaveChangesAsync();
        if (operation == "disable")
            Assert.IsType<OkObjectResult>(await f.Admin().UpdateUserStatus(UserId, new UpdateUserStatusRequest { Status = "inactive" }));
        else
            Assert.IsType<OkObjectResult>(await f.Admin().UpdateUserRole(UserId, new UpdateUserRoleDto { Role = "user" }));
        Assert.True(user.TokenVersion > priorVersion, $"{operation}: TokenVersion stayed {user.TokenVersion}; Program.OnTokenValidated checks only this value and user existence.");
    }

    [Fact]
    public async Task ProductStockRead_ShouldReflectOrderReservationInsteadOfStaleCache()
    {
        using var f = new Fixture();
        var products = new ProductService(new UnitOfWork(f.Db), new ProductRepository(f.Db), f.Cache, f.Db, NullLogger<ProductService>.Instance);
        Assert.Equal(10, (await products.GetProductByIdAsync(ProductId))!.Stock);
        await f.Create();
        var databaseStock = (await f.Db.Products.FindAsync(ProductId))!.Stock;
        Assert.Equal(databaseStock, (await products.GetProductByIdAsync(ProductId))!.Stock);
    }
}
