using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.Infrastructure;
using HuanyuFlowerShop.Repositories;
using HuanyuFlowerShop.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MySql.Data.MySqlClient;
using Xunit;

namespace HuanyuFlowerShop.Tests;

public sealed class IsolatedMySqlFactAttribute : FactAttribute
{
    public IsolatedMySqlFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FLOWERSHOP_TEST_MYSQL")))
            Skip = "Set FLOWERSHOP_TEST_MYSQL to the isolated local test server.";
    }
}

public sealed class MySqlOrderConsistencyTests
{
    private sealed class ScopedEnvironment : IDisposable
    {
        private readonly Dictionary<string, string?> _previous = new();
        public ScopedEnvironment(Dictionary<string, string?> values)
        {
            foreach (var (key, value) in values)
            {
                _previous[key] = Environment.GetEnvironmentVariable(key);
                Environment.SetEnvironmentVariable(key, value);
            }
        }
        public void Dispose()
        {
            foreach (var (key, value) in _previous) Environment.SetEnvironmentVariable(key, value);
        }
    }
    private sealed class Fixture : IAsyncDisposable
    {
        public readonly string Connection;
        public readonly MemoryCache Memory = new(new MemoryCacheOptions());
        public readonly ICacheService Cache;
        private Fixture(string connection)
        {
            Connection = connection;
            Cache = new MemoryCacheService(Memory, NullLogger<MemoryCacheService>.Instance);
        }
        public ApplicationDbContext Context() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseMySQL(Connection).Options);
        public OrderService Orders(ApplicationDbContext db) => new(
            new Repository<Order>(db), new Repository<OrderItem>(db), new Repository<CartItem>(db),
            new Repository<Product>(db), new Repository<OrderStatusHistory>(db), new Repository<Coupon>(db), new Repository<UserCoupon>(db),
            Cache, NullLogger<OrderService>.Instance, Moq.Mock.Of<IPaymentService>(), new ConfigurationBuilder().Build(), new UnitOfWork(db), db);
        public static CreateOrderDto Request(string key, string? coupon = null) => new()
        {
            RecipientName = "Test", Phone = "13800000000", ShippingAddress = "Isolated test address",
            PaymentMethod = "cod", ClientRequestId = key, CouponCode = coupon
        };
        public static async Task<Fixture> Create(int stock = 10, bool secondUser = false, bool createSchema = true)
        {
            var builder = new MySqlConnectionStringBuilder(Environment.GetEnvironmentVariable("FLOWERSHOP_TEST_MYSQL"));
            if (builder.Server != "127.0.0.1" || builder.Port != 13316)
                throw new InvalidOperationException("Integration tests only allow the isolated loopback MySQL server on port 13316.");
            builder.Database = "flowershop_fix_" + Guid.NewGuid().ToString("N");
            builder.AllowUserVariables = true;
            var f = new Fixture(builder.ConnectionString);
            if (!createSchema) return f;
            await using var db = f.Context();
            await db.Database.EnsureCreatedAsync();
            db.Users.Add(new User { Id = 9001, Username = "audit-user-one", Email = "one@example.invalid", Password = "unused" });
            if (secondUser) db.Users.Add(new User { Id = 9002, Username = "audit-user-two", Email = "two@example.invalid", Password = "unused" });
            db.Products.Add(new Product { Id = 9001, Name = "Test flower", Price = 100m, Stock = stock, IsActive = true });
            db.CartItems.Add(new CartItem { UserId = 9001, ProductId = 9001, Price = 100m, Quantity = 1 });
            if (secondUser) db.CartItems.Add(new CartItem { UserId = 9002, ProductId = 9001, Price = 100m, Quantity = 1 });
            await db.SaveChangesAsync();
            return f;
        }
        public async ValueTask DisposeAsync()
        {
            await using var db = Context();
            var builder = new MySqlConnectionStringBuilder(Connection);
            if (!builder.Database.StartsWith("flowershop_fix_", StringComparison.Ordinal)) throw new InvalidOperationException("Refusing to remove a non-test database.");
            await db.Database.EnsureDeletedAsync();
            Memory.Dispose();
        }
    }

    [IsolatedMySqlFact]
    public async Task ConcurrentCheckouts_CannotOversellLastUnit()
    {
        await using var f = await Fixture.Create(stock: 1, secondUser: true);
        async Task<bool> Buy(int user)
        {
            await using var db = f.Context();
            try { await f.Orders(db).CreateOrderAsync(user, Fixture.Request(Guid.NewGuid().ToString("N"))); return true; }
            catch (ArgumentException) { return false; }
        }
        var results = await Task.WhenAll(Buy(9001), Buy(9002));
        Assert.Single(results, x => x);
        await using var check = f.Context();
        Assert.Equal(0, (await check.Products.FindAsync(9001))!.Stock);
        Assert.Equal(1, await check.Orders.CountAsync(o => o.UserId >= 9001));
    }

    [IsolatedMySqlFact]
    public async Task ConcurrentRetries_CreateExactlyOneOrder()
    {
        await using var f = await Fixture.Create();
        var request = Fixture.Request("same-request");
        async Task<int> Buy()
        {
            await using var db = f.Context();
            return (await f.Orders(db).CreateOrderAsync(9001, request)).Id;
        }
        var results = await Task.WhenAll(Buy(), Buy());
        Assert.Equal(results[0], results[1]);
        await using var check = f.Context();
        Assert.Equal(1, await check.Orders.CountAsync(o => o.UserId == 9001));
        Assert.Equal(9, (await check.Products.FindAsync(9001))!.Stock);
        Assert.Empty(await check.CartItems.Where(c => c.UserId == 9001).ToListAsync());
    }

    [IsolatedMySqlFact]
    public async Task ConflictingReuseOfRequestKey_IsRejected()
    {
        await using var f = await Fixture.Create();
        await using (var db = f.Context()) await f.Orders(db).CreateOrderAsync(9001, Fixture.Request("one-key"));
        await using var retry = f.Context();
        var changed = Fixture.Request("one-key");
        changed.ShippingAddress = "Changed address";
        await Assert.ThrowsAsync<ArgumentException>(() => f.Orders(retry).CreateOrderAsync(9001, changed));
    }

    [IsolatedMySqlFact]
    public async Task ConcurrentCancellation_ReleasesInventoryAndCouponOnce()
    {
        await using var f = await Fixture.Create();
        int orderId;
        await using (var db = f.Context())
        {
            db.Coupons.Add(new Coupon { Id = 9001, Code = "TEST10", Value = 10m, Status = "active" });
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            orderId = (await f.Orders(db).CreateOrderAsync(9001, Fixture.Request("coupon-order", "TEST10"))).Id;
        }
        async Task Cancel()
        {
            await using var db = f.Context();
            await f.Orders(db).CancelOrderAsync(9001, orderId);
        }
        await Task.WhenAll(Cancel(), Cancel());
        await using var check = f.Context();
        Assert.Equal(10, (await check.Products.FindAsync(9001))!.Stock);
        Assert.Equal(0, (await check.Coupons.FindAsync(9001))!.UsedCount);
        Assert.Equal("claimed", (await check.UserCoupons.SingleAsync(u => u.UserId == 9001)).Status);
        Assert.Equal(1, await check.OrderStatusHistories.CountAsync(h => h.OrderId == orderId && h.NewStatus == "cancelled"));
    }

    [IsolatedMySqlFact]
    public async Task FailureSavingOrder_RollsBackInventoryCouponAndCart()
    {
        await using var f = await Fixture.Create();
        await using (var setup = f.Context())
        {
            setup.Coupons.Add(new Coupon { Id = 9001, Code = "TEST10", Value = 10m, Status = "active" });
            await setup.SaveChangesAsync();
            await setup.Database.ExecuteSqlRawAsync("CREATE TRIGGER audit_reject_order BEFORE INSERT ON `Orders` FOR EACH ROW SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'forced audit failure'");
        }
        await using (var db = f.Context())
            await Assert.ThrowsAsync<DbUpdateException>(() => f.Orders(db).CreateOrderAsync(9001, Fixture.Request("failed-order", "TEST10")));
        await using var check = f.Context();
        Assert.Equal(10, (await check.Products.FindAsync(9001))!.Stock);
        Assert.Equal(0, (await check.Coupons.FindAsync(9001))!.UsedCount);
        Assert.False(await check.UserCoupons.AnyAsync(u => u.UserId == 9001));
        Assert.Equal(1, await check.CartItems.CountAsync(c => c.UserId == 9001));
        Assert.False(await check.Orders.AnyAsync(o => o.UserId == 9001));
    }

    [IsolatedMySqlFact]
    public async Task ConfirmReceipt_PreservesShippingHistoryAndCompletionTime()
    {
        await using var f = await Fixture.Create();
        int id;
        await using (var db = f.Context()) id = (await f.Orders(db).CreateOrderAsync(9001, Fixture.Request("delivery-order"))).Id;
        await using (var db = f.Context())
        {
            await OrderWorkflow.ChangeStatusAsync(db, f.Cache, id, "processing", null, "admin");
            await OrderWorkflow.ChangeStatusAsync(db, f.Cache, id, "shipped", null, "admin", company: "Test carrier", trackingNumber: "TEST123");
        }
        await using (var db = f.Context()) await f.Orders(db).UpdateOrderStatusAsync(9001, id, "delivered");
        await using (var db = f.Context()) await f.Orders(db).UpdateOrderStatusAsync(9001, id, "delivered");
        await using var restarted = f.Context();
        var order = await f.Orders(restarted).GetOrderByIdAsync(9001, id);
        Assert.Equal("delivered", order!.Status);
        Assert.NotNull(order.DeliveredAt);
        Assert.Equal("TEST123", order.ShippingInfo!.TrackingNumber);
        Assert.Equal(4, await restarted.OrderStatusHistories.CountAsync(h => h.OrderId == id));
        Assert.Equal(115, (await restarted.Users.FindAsync(9001))!.Points);
    }

    [IsolatedMySqlFact]
    public async Task EmptyInitialization_RegistersMigrationsAndRefusesExistingTables()
    {
        await using var f = await Fixture.Create(createSchema: false);
        await using var db = f.Context();
        await EmptyDatabaseInitializer.InitializeAsync(db);
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => EmptyDatabaseInitializer.InitializeAsync(db));
        Assert.Equal(0, await db.Orders.CountAsync());
    }

    [IsolatedMySqlFact]
    public async Task BusinessMigration_UpgradesExistingSchemaWithoutLosingOrder()
    {
        await using var f = await Fixture.Create(createSchema: false);
        await using var db = f.Context();
        await EmptyDatabaseInitializer.InitializeAsync(db);
        db.Users.Add(new User { Id = 9001, Username = "migration-user", Email = "migration@example.invalid", Password = "unused" });
        db.Orders.Add(new Order { Id = 9001, UserId = 9001, OrderNumber = "migration-order", RecipientName = "Existing recipient", RecipientPhone = "13800000000", DeliveryAddress = "Existing address" });
        await db.SaveChangesAsync();
        var migration = db.Database.GetMigrations().Last();
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE `Orders` DROP INDEX `IX_Orders_UserId_ClientRequestId`, DROP COLUMN `ClientRequestId`, DROP COLUMN `RequestFingerprint`, DROP COLUMN `CouponId`");
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM `__EFMigrationsHistory` WHERE `MigrationId` = {migration}");
        db.ChangeTracker.Clear();
        await db.Database.MigrateAsync();
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.Equal("Existing recipient", (await db.Orders.FindAsync(9001))!.RecipientName);
    }

    [IsolatedMySqlFact]
    public async Task RealHttpPipeline_CheckoutDeliveryReviewAndTokenRevocation()
    {
        await using var f = await Fixture.Create(secondUser: true);
        await using (var setup = f.Context())
        {
            var password = BCrypt.Net.BCrypt.HashPassword("LocalAudit123!");
            (await setup.Users.FindAsync(9001))!.Password = password;
            var admin = (await setup.Users.FindAsync(9002))!;
            admin.Password = password;
            admin.Role = "admin";
            setup.Users.Add(new User { Id = 9003, Username = "old-admin", Email = "old-admin@example.invalid", Password = password, Role = "admin" });
            await setup.SaveChangesAsync();
        }
        // Program validates startup configuration before host Build; keep these overrides process-local and restore them.
        using var environment = new ScopedEnvironment(new Dictionary<string, string?>
        {
            ["ConnectionStrings__DefaultConnection"] = f.Connection,
            ["JwtSettings__SecretKey"] = "local-http-only-key-0123456789abcdef",
            ["JwtSettings__Issuer"] = "isolated-test", ["JwtSettings__Audience"] = "isolated-test",
            ["SecuritySettings__AllowOrigins__0"] = "http://localhost"
        });
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = f.Connection,
                ["JwtSettings:SecretKey"] = "local-http-only-key-0123456789abcdef",
                ["JwtSettings:Issuer"] = "isolated-test", ["JwtSettings:Audience"] = "isolated-test",
                ["SecuritySettings:AllowOrigins:0"] = "http://localhost",
                ["Storage:RootPath"] = Path.Combine(Path.GetTempPath(), "flowershop-http-test", Guid.NewGuid().ToString("N")),
                ["Logging:LogLevel:Default"] = "Warning"
            }));
        });
        async Task<HttpClient> Login(string name)
        {
            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", new { username = name, password = "LocalAudit123!" });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", json.GetProperty("token").GetString());
            return client;
        }
        using var buyer = await Login("audit-user-one");
        using var adminClient = await Login("audit-user-two");
        using var oldAdmin = await Login("old-admin");
        Assert.Equal(HttpStatusCode.Forbidden, (await buyer.GetAsync("/api/admin/orders")).StatusCode);
        var quoteResponse = await buyer.PostAsJsonAsync("/api/orders/quote", new OrderQuoteRequest());
        Assert.Equal(HttpStatusCode.OK, quoteResponse.StatusCode);
        Assert.Equal(115m, (await quoteResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("totalAmount").GetDecimal());
        var request = Fixture.Request("http-order");
        request.ExpectedTotal = 115m;
        var placed = await buyer.PostAsJsonAsync("/api/orders", request);
        Assert.Equal(HttpStatusCode.Created, placed.StatusCode);
        var id = (await placed.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        var retry = await buyer.PostAsJsonAsync("/api/orders", request);
        Assert.Equal(id, (await retry.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32());
        Assert.Equal(HttpStatusCode.BadRequest, (await adminClient.DeleteAsync($"/api/admin/orders/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.PutAsJsonAsync($"/api/admin/orders/{id}/status", new { status = "processing" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.PutAsJsonAsync($"/api/admin/orders/{id}/shipping", new { company = "Test carrier", trackingNumber = "HTTP123" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await buyer.PutAsync($"/api/orders/{id}/confirm-receipt", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await buyer.PostAsJsonAsync("/api/ProductReview", new { productId = 9001, rating = 5, comment = "HTTP flow verified" })).StatusCode);
        var final = await (await buyer.GetAsync($"/api/orders/{id}")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("delivered", final.GetProperty("status").GetString());
        Assert.Equal("HTTP123", final.GetProperty("shippingInfo").GetProperty("trackingNumber").GetString());
        Assert.Equal(HttpStatusCode.OK, (await adminClient.PutAsJsonAsync("/api/admin/users/9003/role", new { role = "user" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await oldAdmin.GetAsync("/api/admin/orders")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminClient.PutAsJsonAsync("/api/admin/users/9001/status", new { status = "inactive" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await buyer.GetAsync("/api/orders")).StatusCode);
    }
}
