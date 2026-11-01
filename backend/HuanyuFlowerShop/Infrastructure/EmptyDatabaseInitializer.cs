using HuanyuFlowerShop.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HuanyuFlowerShop.Infrastructure;

/// <summary>历史最初两份迁移为空；新环境使用当前模型初始化，禁止用于已有业务库。</summary>
public static class EmptyDatabaseInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext db)
    {
        var creator = db.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>();
        if (!await creator.ExistsAsync()) await creator.CreateAsync();
        if (await creator.HasTablesAsync())
            throw new InvalidOperationException("空库初始化已拒绝：数据库中已有表。已有数据库请使用迁移升级。");
        await db.Database.EnsureCreatedAsync();
        var history = db.GetService<IHistoryRepository>();
        await db.Database.ExecuteSqlRawAsync(history.GetCreateIfNotExistsScript());
        foreach (var migration in db.Database.GetMigrations())
            await db.Database.ExecuteSqlRawAsync(history.GetInsertScript(new HistoryRow(migration, "9.0.9")));
    }
}
