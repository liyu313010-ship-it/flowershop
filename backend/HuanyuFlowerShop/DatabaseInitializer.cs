using Microsoft.EntityFrameworkCore;
using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;

namespace HuanyuFlowerShop
{
    public class DatabaseInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // 数据库已通过Migrate()创建，不再需要EnsureCreated()

                // 创建管理员账号（如果不存在）
                if (!context.Users.Any(u => u.Username == "admin"))
                {
                    var adminUser = new User
                    {
                        Username = "admin",
                        Email = "admin@huanyu.com",
                        Phone = "13800138000",
                        FullName = "系统管理员",
                        Role = "admin",
                        Status = "active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    // 使用BCrypt进行密码哈希
                    adminUser.Password = BCrypt.Net.BCrypt.HashPassword("admin@123");
                    
                    context.Users.Add(adminUser);
                    context.SaveChanges();
                }



                // 检查是否已有分类数据
                if (!context.Categories.Any())
                {
                    // 添加分类数据
                    var categories = new Category[]
                    {
                        new Category
                        {
                            Name = "玫瑰",
                            Description = "浪漫爱情的象征",
                            SortOrder = 1,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Category
                        {
                            Name = "康乃馨",
                            Description = "温馨感恩的选择",
                            SortOrder = 2,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Category
                        {
                            Name = "百合",
                            Description = "纯洁优雅的代表",
                            SortOrder = 3,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Category
                        {
                            Name = "郁金香",
                            Description = "高贵优雅的花中皇后",
                            SortOrder = 4,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Category
                        {
                            Name = "向日葵",
                            Description = "阳光明媚，充满活力",
                            SortOrder = 5,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Category
                        {
                            Name = "满天星",
                            Description = "细腻浪漫，烘托氛围",
                            SortOrder = 6,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    context.Categories.AddRange(categories);
                    context.SaveChanges();
                }

                var mustHave = new[]
                {
                    new { Name = "玫瑰", Description = "浪漫爱情的象征" },
                    new { Name = "康乃馨", Description = "温馨感恩的选择" },
                    new { Name = "百合", Description = "纯洁优雅的代表" },
                    new { Name = "郁金香", Description = "高贵优雅的花中皇后" },
                    new { Name = "向日葵", Description = "阳光明媚，充满活力" },
                    new { Name = "满天星", Description = "细腻浪漫，烘托氛围" },
                    new { Name = "荷花", Description = "出淤泥而不染" },
                    new { Name = "茉莉", Description = "清新雅致，香气怡人" },
                    new { Name = "桂花", Description = "金秋飘香，温润如玉" },
                    new { Name = "樱花", Description = "灿若云霞，春日浪漫" },
                    new { Name = "栀子花", Description = "洁白芬芳，纯净之美" },
                    new { Name = "杜鹃", Description = "千姿百态，色彩斑斓" },
                    new { Name = "牡丹", Description = "花中之王，雍容华贵" },
                    new { Name = "组合花束", Description = "多花材组合花束" }
                };

                var existingNames = context.Categories.Select(c => c.Name).ToHashSet();
                var currentSort = context.Categories.Any() ? context.Categories.Max(c => c.SortOrder) : 0;
                foreach (var m in mustHave)
                {
                    if (!existingNames.Contains(m.Name))
                    {
                        context.Categories.Add(new Category
                        {
                            Name = m.Name,
                            Description = m.Description,
                            SortOrder = ++currentSort,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
                context.SaveChanges();

                // 仅在初始数据库设置时创建示例产品，不再自动重新创建已删除的产品

                // 检查是否已有产品数据
                if (!context.Products.Any())
                {
                    // 添加示例产品
                    var products = new Product[]
                    {
                        new Product
                        {
                            Name = "玫瑰花束",
                            Description = "新鲜红玫瑰，表达爱意的完美选择",
                            Price = 99.99m,
                            Stock = 50,
                            ImageUrl = "/images/rose-bouquet.svg",
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        },
                        new Product
                        {
                            Name = "向日葵花束",
                            Description = "阳光明媚的向日葵，带来温暖和快乐",
                            Price = 79.99m,
                            Stock = 30,
                            ImageUrl = "/images/sunflower.svg",
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        },
                        new Product
                        {
                            Name = "百合花束",
                            Description = "优雅的百合花，象征纯洁和高贵",
                            Price = 89.99m,
                            Stock = 40,
                            ImageUrl = "/images/lily.svg",
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        },
                        new Product
                        {
                            Name = "康乃馨花束",
                            Description = "温馨的康乃馨，适合送给母亲和长辈",
                            Price = 69.99m,
                            Stock = 35,
                            ImageUrl = "/images/pink-carnation.svg",
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        },
                        new Product
                        {
                            Name = "满天星花束",
                            Description = "浪漫的满天星，象征纯洁的爱情",
                            Price = 59.99m,
                            Stock = 25,
                            ImageUrl = "/images/baby-breath.svg",
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        }
                    };

                    context.Products.AddRange(products);
                    context.SaveChanges();

                    // 为示例产品映射分类ID（如果尚未设置）
                    var categoryMap = context.Categories
                        .Select(c => new { c.Id, c.Name })
                        .ToList()
                        .ToDictionary(c => c.Name, c => c.Id);

                    var productsWithoutCategory = context.Products
                        .Where(p => p.CategoryId == null)
                        .ToList();

                    foreach (var p in productsWithoutCategory)
                    {
                        if (p.Name.Contains("玫瑰"))
                        {
                            p.CategoryId = categoryMap.TryGetValue("玫瑰", out var id) ? id : p.CategoryId;
                        }
                        else if (p.Name.Contains("康乃馨"))
                        {
                            p.CategoryId = categoryMap.TryGetValue("康乃馨", out var id) ? id : p.CategoryId;
                        }
                        else if (p.Name.Contains("百合"))
                        {
                            p.CategoryId = categoryMap.TryGetValue("百合", out var id) ? id : p.CategoryId;
                        }
                        else if (p.Name.Contains("郁金香"))
                        {
                            p.CategoryId = categoryMap.TryGetValue("郁金香", out var id) ? id : p.CategoryId;
                        }
                        else if (p.Name.Contains("向日葵"))
                        {
                            p.CategoryId = categoryMap.TryGetValue("向日葵", out var id) ? id : p.CategoryId;
                        }
                        else if (p.Name.Contains("满天星"))
                        {
                            p.CategoryId = categoryMap.TryGetValue("满天星", out var id) ? id : p.CategoryId;
                        }
                        else if (p.Name.Contains("荷花"))
                        {
                            p.CategoryId = categoryMap.TryGetValue("荷花", out var id) ? id : p.CategoryId;
                        }
                        else if (p.Name.Contains("茉莉"))
                        {
                            p.CategoryId = categoryMap.TryGetValue("茉莉", out var id) ? id : p.CategoryId;
                        }
                        else if (p.Name.Contains("桂花"))
                        {
                            p.CategoryId = categoryMap.TryGetValue("桂花", out var id) ? id : p.CategoryId;
                        }
                        else if (p.Name.Contains("樱花"))
                        {
                            p.CategoryId = categoryMap.TryGetValue("樱花", out var id) ? id : p.CategoryId;
                        }
                        else if (p.Name.Contains("栀子花"))
                        {
                            p.CategoryId = categoryMap.TryGetValue("栀子花", out var id) ? id : p.CategoryId;
                        }
                        else if (p.Name.Contains("杜鹃"))
                        {
                            p.CategoryId = categoryMap.TryGetValue("杜鹃", out var id) ? id : p.CategoryId;
                        }
                        else if (p.Name.Contains("牡丹"))
                        {
                            p.CategoryId = categoryMap.TryGetValue("牡丹", out var id) ? id : p.CategoryId;
                        }
                    }

                    context.SaveChanges();
                }

                // 修复已有数据：为缺少分类的产品补全CategoryId
                try
                {
                    var categoryMapFix = context.Categories
                        .Select(c => new { c.Id, c.Name })
                        .ToList()
                        .ToDictionary(c => c.Name, c => c.Id);

                    var needFix = context.Products
                        .Where(p => p.CategoryId == null)
                        .ToList();

                    foreach (var p in needFix)
                    {
                        if (p.Name.Contains("玫瑰"))
                            p.CategoryId = categoryMapFix.TryGetValue("玫瑰", out var id) ? id : p.CategoryId;
                        else if (p.Name.Contains("康乃馨"))
                            p.CategoryId = categoryMapFix.TryGetValue("康乃馨", out var id) ? id : p.CategoryId;
                        else if (p.Name.Contains("百合"))
                            p.CategoryId = categoryMapFix.TryGetValue("百合", out var id) ? id : p.CategoryId;
                        else if (p.Name.Contains("郁金香"))
                            p.CategoryId = categoryMapFix.TryGetValue("郁金香", out var id) ? id : p.CategoryId;
                    }

                    if (needFix.Count > 0)
                        context.SaveChanges();
                }
                catch {}
            }
        }
    }
}
