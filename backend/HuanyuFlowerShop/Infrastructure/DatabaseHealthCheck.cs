using HuanyuFlowerShop.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace HuanyuFlowerShop.Infrastructure;

public sealed class DatabaseHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            if (!await db.Database.CanConnectAsync(cancellationToken)) return HealthCheckResult.Unhealthy("Database unavailable");
            await db.Users.AsNoTracking().Select(u => new { u.Id, u.TokenVersion }).Take(1).ToListAsync(cancellationToken);
            await db.Orders.AsNoTracking().Select(o => new { o.Id, o.ClientRequestId }).Take(1).ToListAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch { return HealthCheckResult.Unhealthy("Database unavailable"); }
    }
}
