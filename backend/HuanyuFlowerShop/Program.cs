using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
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
using HuanyuFlowerShop.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 数据库配置
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("数据库连接字符串未配置，请通过环境变量 ConnectionStrings__DefaultConnection 或密钥管理服务注入");
}
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    options.UseMySQL(connectionString, mySqlOptions =>
    {
        mySqlOptions.CommandTimeout(30);
    });
});

// JWT配置
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["SecretKey"];
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32 || string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException("JWT 配置无效：SecretKey 至少需要 32 个字符，并配置 Issuer/Audience");
}

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
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var id = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var version = context.Principal?.FindFirst("token_version")?.Value;
                if (!int.TryParse(id, out var userId) || !int.TryParse(version, out var tokenVersion))
                {
                    context.Fail("token version missing");
                    return;
                }
                var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                var currentVersion = await db.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => (int?)u.TokenVersion).FirstOrDefaultAsync();
                if (!currentVersion.HasValue || currentVersion.Value != tokenVersion)
                    context.Fail("token revoked");
            }
        };
    });

builder.Services.AddAuthorization();

// CORS配置 - 从配置文件读取允许的源
var securitySettings = builder.Configuration.GetSection("SecuritySettings");
var allowOrigins = securitySettings.GetSection("AllowOrigins").Get<string[]>() ?? Array.Empty<string>();
if (allowOrigins.Length == 0)
{
    throw new InvalidOperationException("SecuritySettings:AllowOrigins 未配置，禁止使用开放式 CORS");
}

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
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();
    builder.Services.AddScoped<IOrderStatusHistoryService, OrderStatusHistoryService>();
        builder.Services.AddScoped<IAuditLogService, AuditLogService>();
        builder.Services.AddScoped<IProductReviewService, ProductReviewService>();


// 注册Category仓储
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// 注册Product仓储
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// DTO 映射采用显式映射，避免引入存在安全公告的 AutoMapper 运行时依赖。
builder.Services.AddHostedService<ExpiredOrderCleanupService>();

var app = builder.Build();

// 数据库迁移由发布流程执行，应用启动不再静默修改生产数据库。
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    if (app.Environment.IsDevelopment())
    {
        context.Database.Migrate();
    }
    else
    {
        app.Logger.LogInformation("生产环境跳过自动迁移，请在发布前执行 dotnet ef database update 或审批后的迁移脚本");
    }

    if (app.Environment.IsDevelopment() || builder.Configuration.GetValue("DatabaseInitializer:Enabled", false))
    {
        DatabaseInitializer.Initialize(services);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 添加全局异常处理中间件
app.UseMiddleware<GlobalExceptionHandler>();
app.UseForwardedHeaders();

// 添加安全头
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    var csp = app.Environment.IsDevelopment()
        ? "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self' http://localhost:5173 http://localhost:5002 ws://localhost:5173"
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

app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
// 身份认证后限流，才能按用户身份区分请求；代理环境使用转发后的真实 IP。
app.UseMiddleware<RateLimitingMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true
});

app.Run();
