using HuanyuFlowerShop.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HuanyuFlowerShop.Infrastructure;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _db;

    public DatabaseHealthCheck(ApplicationDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _db.Database.CanConnectAsync(cancellationToken))
                return HealthCheckResult.Unhealthy("database unavailable");

            // 同时验证应用依赖的最新字段，防止“数据库能连接但迁移漏执行”。
            await _db.Users.AsNoTracking()
                .Select(u => new { u.Id, u.EmailVerified, u.Points, u.TokenVersion })
                .Take(1)
                .ToListAsync(cancellationToken);
            return HealthCheckResult.Healthy("database reachable and schema current");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("database check failed", ex);
        }
    }
}
