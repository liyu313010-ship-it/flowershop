using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Services;
using HuanyuFlowerShop.Repositories;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.Middleware;
using FluentValidation.AspNetCore;
using FluentValidation;
using HuanyuFlowerShop.Validators;
using HuanyuFlowerShop;
using Microsoft.AspNetCore.SignalR;
using HuanyuFlowerShop.Hubs;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 数据库配置
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("数据库连接字符串未配置");
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    options.UseMySQL(connectionString, mySqlOptions =>
    {
        mySqlOptions.CommandTimeout(30);
    });
});

// JWT配置
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Key is not configured");
var jwtIssuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
var jwtAudience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// CORS配置 - 从配置文件读取允许的源
var securitySettings = builder.Configuration.GetSection("SecuritySettings");
var allowOrigins = securitySettings.GetSection("AllowOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});

// 添加FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateProductDtoValidator>();

// 添加内存缓存
builder.Services.AddMemoryCache();

// 添加响应压缩
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

// 添加HSTS
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(30);
});

// 注册缓存服务
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// 添加速率限制
builder.Services.AddSingleton<IRateLimiter, MemoryRateLimiter>();

// 注册仓储
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// 注册工作单元
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 注册服务
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<IFavoriteService, FavoriteService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
    builder.Services.AddScoped<IOrderStatusHistoryService, OrderStatusHistoryService>();
        builder.Services.AddScoped<IAuditLogService, AuditLogService>();
            builder.Services.AddScoped<IProductReviewService, ProductReviewService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IVideoService, VideoService>();

// 注册SignalR服务
builder.Services.AddSignalR();


// 注册Category仓储
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// 注册Product仓储
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// 使用默认或环境变量配置的地址（ASPNETCORE_URLS）以避免重复绑定

// 初始化数据库 - 暂时禁用迁移，因为表已存在
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<ApplicationDbContext>();
    // 初始化数据库
    // 开发环境下启用自动迁移
    if (app.Environment.IsDevelopment())
    {
        // 自动应用迁移
        context.Database.Migrate();
        // 开发环境下若迁移缺失，安全创建聊天相关表
        try
        {
            context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS `Videos` (
                `Id` INT NOT NULL AUTO_INCREMENT,
                `Title` VARCHAR(200) NOT NULL,
                `FilePath` VARCHAR(500) NOT NULL,
                `Slot` VARCHAR(50) NOT NULL DEFAULT 'story',
                `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
                `CreatedAt` DATETIME NOT NULL,
                `UpdatedAt` DATETIME NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
            try { context.Database.ExecuteSqlRaw(@"ALTER TABLE `Videos` ADD COLUMN `Slot` VARCHAR(50) NOT NULL DEFAULT 'story'"); } catch { }

            context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS `OrderStatusHistories` (
                `Id` INT NOT NULL AUTO_INCREMENT,
                `OrderId` INT NOT NULL,
                `OldStatus` VARCHAR(20) NULL,
                `NewStatus` VARCHAR(20) NOT NULL,
                `OperatorId` INT NULL,
                `OperatorName` VARCHAR(50) NOT NULL,
                `Note` VARCHAR(500) NULL,
                `CreatedAt` DATETIME NOT NULL,
                PRIMARY KEY (`Id`),
                INDEX `IX_OrderStatusHistories_OrderId` (`OrderId`),
                CONSTRAINT `FK_OrderStatusHistories_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `Orders` (`Id`) ON DELETE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
        }
        catch { }
    }
    else
    {
        // 生产环境下使用安全的SQL语句确保数据库结构正确
        try
        {
            context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS `Videos` (
                `Id` INT NOT NULL AUTO_INCREMENT,
                `Title` VARCHAR(200) NOT NULL,
                `FilePath` VARCHAR(500) NOT NULL,
                `Slot` VARCHAR(50) NOT NULL DEFAULT 'story',
                `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
                `CreatedAt` DATETIME NOT NULL,
                `UpdatedAt` DATETIME NULL,
                PRIMARY KEY (`Id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
            try { context.Database.ExecuteSqlRaw(@"ALTER TABLE `Videos` ADD COLUMN `Slot` VARCHAR(50) NOT NULL DEFAULT 'story'"); } catch { }
            // 标记旧迁移为已应用，避免 EF 在后续 update 时重复创建已存在表
            try
            {
                context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
                    `MigrationId` VARCHAR(150) NOT NULL,
                    `ProductVersion` VARCHAR(32) NOT NULL,
                    PRIMARY KEY (`MigrationId`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
                context.Database.ExecuteSqlRaw(@"INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) VALUES
                    ('20251130045107_InitialMySQLMigration','9.0.0'),
                    ('20251130045257_MySQLCleanMigration','9.0.0'),
                    ('20251201084240_AddPaymentFieldsToOrder','9.0.0'),
                    ('20251209000000_AddChatTables','9.0.0'),
                    ('20251211133513_AddVideosTable','9.0.0')
                ;");
            }
            catch {}

            // 安全创建OrderStatusHistories表（如果不存在）
            context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS `OrderStatusHistories` (
                `Id` INT NOT NULL AUTO_INCREMENT,
                `OrderId` INT NOT NULL,
                `OldStatus` VARCHAR(20) NULL,
                `NewStatus` VARCHAR(20) NOT NULL,
                `OperatorId` INT NULL,
                `OperatorName` VARCHAR(50) NOT NULL,
                `Note` VARCHAR(500) NULL,
                `CreatedAt` DATETIME NOT NULL,
                PRIMARY KEY (`Id`),
                INDEX `IX_OrderStatusHistories_OrderId` (`OrderId`),
                CONSTRAINT `FK_OrderStatusHistories_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `Orders` (`Id`) ON DELETE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

            // 安全创建Coupons表（如果不存在）
            context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS `Coupons` (
                `Id` INT NOT NULL AUTO_INCREMENT,
                `Code` VARCHAR(50) NOT NULL,
                `DiscountType` VARCHAR(20) NOT NULL,
                `Value` DECIMAL(10,2) NOT NULL,
                `MinOrderAmount` DECIMAL(10,2) NOT NULL,
                `MaxDiscount` DECIMAL(10,2) NULL,
                `UsageLimit` INT NULL,
                `UsageLimitPerUser` INT NULL,
                `UsedCount` INT NOT NULL DEFAULT 0,
                `Status` VARCHAR(20) NOT NULL,
                `StartAt` DATETIME NULL,
                `EndAt` DATETIME NULL,
                `CreatedAt` DATETIME NOT NULL,
                `UpdatedAt` DATETIME NULL,
                PRIMARY KEY (`Id`),
                UNIQUE INDEX `IX_Coupons_Code` (`Code`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

            // 安全创建UserCoupons表（如果不存在）
            context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS `UserCoupons` (
                `Id` INT NOT NULL AUTO_INCREMENT,
                `UserId` INT NOT NULL,
                `CouponId` INT NOT NULL,
                `ClaimedAt` DATETIME NOT NULL,
                `UsedAt` DATETIME NULL,
                `Status` VARCHAR(20) NOT NULL,
                PRIMARY KEY (`Id`),
                INDEX `IX_UserCoupons_UserId` (`UserId`),
                INDEX `IX_UserCoupons_CouponId` (`CouponId`),
                UNIQUE INDEX `IX_UserCoupons_UserId_CouponId` (`UserId`, `CouponId`),
                CONSTRAINT `FK_UserCoupons_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
                CONSTRAINT `FK_UserCoupons_Coupons_CouponId` FOREIGN KEY (`CouponId`) REFERENCES `Coupons` (`Id`) ON DELETE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

            // 聊天相关表（如果不存在）
            try
            {
                context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS `Conversations` (
                    `Id` INT NOT NULL AUTO_INCREMENT,
                    `UserId` INT NOT NULL,
                    `AdminId` INT NOT NULL,
                    `LastMessage` TEXT NULL,
                    `LastMessageTime` DATETIME NULL,
                    `UnreadCount` INT NOT NULL DEFAULT 0,
                    `CreatedAt` DATETIME NOT NULL,
                    `UpdatedAt` DATETIME NOT NULL,
                    PRIMARY KEY (`Id`),
                    INDEX `IX_Conversations_UserId` (`UserId`),
                    INDEX `IX_Conversations_AdminId` (`AdminId`),
                    CONSTRAINT `FK_Conversations_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
                    CONSTRAINT `FK_Conversations_Users_AdminId` FOREIGN KEY (`AdminId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

                context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS `Messages` (
                    `Id` INT NOT NULL AUTO_INCREMENT,
                    `SenderId` INT NOT NULL,
                    `ReceiverId` INT NOT NULL,
                    `Content` TEXT NOT NULL,
                    `MessageType` VARCHAR(20) NOT NULL,
                    `IsRead` TINYINT(1) NOT NULL DEFAULT 0,
                    `CreatedAt` DATETIME NOT NULL,
                    `UpdatedAt` DATETIME NOT NULL,
                    PRIMARY KEY (`Id`),
                    INDEX `IX_Messages_SenderId` (`SenderId`),
                    INDEX `IX_Messages_ReceiverId` (`ReceiverId`),
                    CONSTRAINT `FK_Messages_Users_SenderId` FOREIGN KEY (`SenderId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
                    CONSTRAINT `FK_Messages_Users_ReceiverId` FOREIGN KEY (`ReceiverId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
            }
            catch { }

            // 安全添加Addresses表的缺失列
            try
            {
                context.Database.ExecuteSqlRaw(@"ALTER TABLE `Addresses` ADD COLUMN `PhoneNumber` varchar(20) NOT NULL DEFAULT ''");
            }
            catch { }
        try
        {
            context.Database.ExecuteSqlRaw(@"ALTER TABLE `Addresses` ADD COLUMN `PostalCode` varchar(20) NULL");
        }
        catch { }
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "数据库初始化失败");
        }
    }

    // 初始化数据库数据（分类/产品种子与缺失分类修复）
DatabaseInitializer.Initialize(services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 添加全局异常处理中间件
app.UseMiddleware<GlobalExceptionHandler>();

// 添加安全头
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    var csp = app.Environment.IsDevelopment()
        ? "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self' http://localhost:5173 http://127.0.0.1:5173 http://localhost:5176 http://127.0.0.1:5176 http://localhost:* ws://localhost:5173 ws://localhost:5176"
        : "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'";
    context.Response.Headers.Append("Content-Security-Policy", csp);
    await next();
});

// 暂时注释掉HTTPS重定向，因为我们使用HTTP端口
// app.UseHttpsRedirection();

// 启用HSTS（生产环境）
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// 启用响应压缩
app.UseResponseCompression();

// 启用速率限制中间件
app.UseMiddleware<RateLimitingMiddleware>();

// 启用审计日志中间件
app.UseMiddleware<AuditLogMiddleware>();

// 启用静态文件服务
app.UseStaticFiles();

// 为uploads目录配置静态文件服务
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// 额外映射本地用户头像目录为 /uploads/avatars
var userAvatarPath = @"D:\flowershop\用户头像";
if (Directory.Exists(userAvatarPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(userAvatarPath),
        RequestPath = "/uploads/avatars"
    });
}

app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 添加SignalR Hub路由
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
