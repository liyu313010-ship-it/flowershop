using System;
using System.Threading.Tasks;
using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace HuanyuFlowerShop
{
    class CreateAdminUser
    {
        static async Task Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySQL(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ?? throw new InvalidOperationException("请配置数据库连接环境变量"));

            using (var context = new ApplicationDbContext(optionsBuilder.Options))
            {
                // 检查是否已存在admin用户
                var adminUser = await context.Users
                    .FirstOrDefaultAsync(u => u.Username == "admin");

                if (adminUser == null)
                {
                    // 创建admin用户
                    adminUser = new User
                    {
                        Username = "admin",
                        Email = "admin@example.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Role = "admin",
                        Status = "active",
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Users.Add(adminUser);
                    await context.SaveChangesAsync();
                    Console.WriteLine("Admin user created successfully!");
                }
                else
                {
                    Console.WriteLine("Admin user already exists!");
                }
            }
        }
    }
}