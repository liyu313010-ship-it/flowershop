using Microsoft.EntityFrameworkCore;
using HuanyuFlowerShop.Entities;

namespace HuanyuFlowerShop.Data
{
    /// <summary>
    /// 应用程序数据库上下文
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // 实体集
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Address> Addresses { get; set; } = null!;
        public DbSet<Favorite> Favorites { get; set; } = null!;
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; } = null!;
        public DbSet<ProductReview> ProductReviews { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<Coupon> Coupons { get; set; } = null!;
        public DbSet<UserCoupon> UserCoupons { get; set; } = null!;
        public DbSet<ProductRecommendation> ProductRecommendations { get; set; } = null!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置User实体
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Password).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Gender).HasMaxLength(20);
                entity.Property(e => e.EmailVerified).HasDefaultValue(false).IsRequired();
                entity.Property(e => e.Points).HasDefaultValue(0).IsRequired();
                entity.Property(e => e.TokenVersion).HasDefaultValue(0).IsRequired();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TokenHash).IsUnique();
                entity.HasIndex(e => new { e.UserId, e.ExpiresAt });
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EmailVerificationToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TokenHash).IsUnique();
                entity.HasIndex(e => new { e.UserId, e.ExpiresAt });
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置Category实体
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.HasIndex(e => e.SortOrder);
                entity.HasIndex(e => e.IsActive);
            });

            // 配置Product实体
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.Size).HasMaxLength(50);
                entity.Property(e => e.Material).HasMaxLength(100);
                entity.Property(e => e.Occasion).HasMaxLength(100);
                entity.HasIndex(e => e.IsFeatured);
                entity.HasIndex(e => e.IsActive);
                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // 配置CartItem实体
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.CartItems)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.CartItems)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
                // 确保同一用户同一商品只有一个购物车项
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
            });

            // 配置Order实体
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.OrderNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.RecipientName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.RecipientPhone).HasMaxLength(20).IsRequired();
                entity.Property(e => e.DeliveryAddress).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Message).HasMaxLength(500);
                entity.Property(e => e.ShippingMethod).HasMaxLength(30).IsRequired();
                entity.Property(e => e.SenderName).HasMaxLength(50);
                entity.Property(e => e.CardMessage).HasMaxLength(500);
                entity.Property(e => e.SubstitutionPreference).HasMaxLength(30).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
                entity.Property(e => e.PaymentMethod).HasMaxLength(20).IsRequired();
                entity.Property(e => e.PaymentStatus).HasMaxLength(20).IsRequired();
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置Address实体
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.RecipientName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Province).HasMaxLength(50).IsRequired();
                entity.Property(e => e.City).HasMaxLength(50).IsRequired();
                entity.Property(e => e.District).HasMaxLength(50).IsRequired();
                entity.Property(e => e.DetailAddress).HasMaxLength(200).IsRequired();
                // PostalCode 目前可选，如数据库无此列则由启动脚本补充或忽略
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.IsDefault).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Addresses)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置OrderItem实体
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.ProductName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ProductImage).HasMaxLength(500);
                entity.HasOne(e => e.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置Favorite实体
            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Favorites)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Favorites)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
                // 确保同一用户不能重复收藏同一商品
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
            });

            // 配置OrderStatusHistory实体
            modelBuilder.Entity<OrderStatusHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.OldStatus).HasMaxLength(20);
                entity.Property(e => e.NewStatus).HasMaxLength(20).IsRequired();
                entity.Property(e => e.OperatorName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Note).HasMaxLength(500);
                entity.HasOne(e => e.Order)
                      .WithMany(o => o.StatusHistories)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // 配置ProductReview实体
            modelBuilder.Entity<ProductReview>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Rating).IsRequired();
                entity.Property(e => e.Comment).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.ProductReviews)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.ProductReviews)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.Rating);
            });

            // 配置AuditLog实体
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                // 明确指定Id字段为自增主键
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
                // 确保UserId属性正确配置为可空整数类型
                entity.Property(e => e.UserId)
                    .IsRequired(false); // 显式设置为可空
                // 确保各属性的StringLength与实体类一致
                entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Resource).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ResourceId).HasMaxLength(100);
                entity.Property(e => e.Details).HasMaxLength(1000);
                entity.Property(e => e.IPAddress).HasMaxLength(45); // 确保IPAddress长度正确配置
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.Resource);
                entity.HasIndex(e => e.CreatedAt);
            });

            // 配置Coupon实体
            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.DiscountType).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
                entity.Property(e => e.UsageLimit);
                entity.Property(e => e.UsageLimitPerUser);
            });

            // 配置UserCoupon实体
            modelBuilder.Entity<UserCoupon>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
                entity.HasIndex(e => new { e.UserId, e.CouponId }).IsUnique();
            });

            // 配置ProductRecommendation实体
            modelBuilder.Entity<ProductRecommendation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.ForUserId);
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.GeneratedAt);
            });

            // 注意：种子数据已移至独立的SeedData类中
        }
    }
}
