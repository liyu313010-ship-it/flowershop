using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HuanyuFlowerShop.Exceptions;

namespace HuanyuFlowerShop.Entities
{
    /// <summary>
    /// 鲜花商品实体类
    /// </summary>
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [StringLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string? ImageUrl { get; set; }
        
        /// <summary>
        /// 产品评价集合
        /// </summary>
        public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

        /// <summary>
        /// 库存数量
        /// </summary>
        [Required]
        public int Stock { get; set; }

        /// <summary>
        /// 是否推荐到首页
        /// </summary>
        [Required]
        public bool IsFeatured { get; set; } = false;

        /// <summary>
        /// 是否上架
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 销售数量
        /// </summary>
        public int SalesCount { get; set; } = 0;

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? Size { get; set; }

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? Material { get; set; }

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? Occasion { get; set; }

        /// <summary>
        /// 分类ID
        /// </summary>
        public int? CategoryId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // 注意：暂时注释掉IsDeleted字段，因为数据库中不存在该列
        // 后续可通过数据库迁移添加此列
        // /// <summary>
        // /// 是否已删除
        // /// </summary>
        // public bool IsDeleted { get; set; } = false;

        // 导航属性
        public virtual Category? Category { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

        // 业务方法

        /// <summary>
        /// 减少库存
        /// </summary>
        /// <param name="quantity">要减少的数量</param>
        /// <returns>是否成功减少库存</returns>
        /// <exception cref="ArgumentException">当数量小于等于0时抛出</exception>
        public bool ReduceStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("数量必须大于0");

            if (Stock < quantity)
                return false;

            Stock -= quantity;
            SalesCount += quantity;
            UpdatedAt = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// 检查库存是否充足
        /// </summary>
        /// <param name="quantity">需要的数量</param>
        /// <returns>库存是否充足</returns>
        public bool HasEnoughStock(int quantity)
        {
            return Stock >= quantity;
        }

        /// <summary>
        /// 更新价格
        /// </summary>
        /// <param name="newPrice">新价格</param>
        /// <exception cref="ArgumentException">当价格小于等于0时抛出</exception>
        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice <= 0)
                throw new ArgumentException("价格必须大于0");

            Price = newPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// 增加库存
        /// </summary>
        /// <param name="quantity">要增加的数量</param>
        /// <exception cref="ArgumentException">当数量小于等于0时抛出</exception>
        public void AddStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("数量必须大于0");

            Stock += quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// 设置为推荐商品
        /// </summary>
        public void SetAsFeatured()
        {
            IsFeatured = true;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// 取消推荐状态
        /// </summary>
        public void UnsetFeatured()
        {
            IsFeatured = false;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// 上架商品
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// 下架商品
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// 获取商品状态描述
        /// </summary>
        /// <returns>商品状态描述</returns>
        public string GetStatusDescription()
        {
            if (!IsActive)
                return "已下架";
            
            if (Stock <= 0)
                return "缺货";
            
            if (Stock <= 10)
                return "库存紧张";
            
            return "正常销售";
        }

        /// <summary>
        /// 检查是否可以购买
        /// </summary>
        /// <param name="quantity">购买数量</param>
        /// <returns>是否可以购买</returns>
        public bool CanBePurchased(int quantity)
        {
            return IsActive && HasEnoughStock(quantity);
        }
    }
}