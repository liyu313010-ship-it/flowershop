using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace HuanyuFlowerShop.Tests;

public class RateLimitingMiddlewareTests
{
    [Fact]
    public async Task Logout_IsNotRateLimited()
    {
        var limiter = new RecordingRateLimiter { Allow = false };
        var called = false;
        var middleware = CreateMiddleware(limiter, _ =>
        {
            called = true;
            return Task.CompletedTask;
        });
        var context = CreateContext("POST", "/api/auth/logout");

        await middleware.InvokeAsync(context);

        Assert.True(called);
        Assert.Empty(limiter.Attempts);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task ChatUnreadCount_UsesExpandedAdminLimit()
    {
        var limiter = new RecordingRateLimiter { Allow = true };
        var middleware = CreateMiddleware(limiter);
        var context = CreateContext("GET", "/api/chat/unread-count");

        await middleware.InvokeAsync(context);

        var attempt = Assert.Single(limiter.Attempts);
        Assert.StartsWith("admin_ip_", attempt.Key);
        Assert.Equal(240, attempt.Limit);
    }

    [Fact]
    public async Task FavoriteList_UsesExpandedReadLimit()
    {
        var limiter = new RecordingRateLimiter { Allow = true };
        var middleware = CreateMiddleware(limiter);
        var context = CreateContext("GET", "/api/Favorite/list");

        await middleware.InvokeAsync(context);

        var attempt = Assert.Single(limiter.Attempts);
        Assert.StartsWith("read_ip_", attempt.Key);
        Assert.Equal(360, attempt.Limit);
    }

    [Fact]
    public async Task Login_UsesStricterAuthLimit()
    {
        var limiter = new RecordingRateLimiter { Allow = false };
        var middleware = CreateMiddleware(limiter);
        var context = CreateContext("POST", "/api/auth/login");

        await middleware.InvokeAsync(context);

        var attempt = Assert.Single(limiter.Attempts);
        Assert.StartsWith("auth_ip_", attempt.Key);
        Assert.Equal(30, attempt.Limit);
        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
    }

    private static RateLimitingMiddleware CreateMiddleware(RecordingRateLimiter limiter, RequestDelegate? next = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SecuritySettings:RateLimitPerMinute"] = "60"
            })
            .Build();
        return new RateLimitingMiddleware(next ?? (_ => Task.CompletedTask), limiter, configuration);
    }

    private static DefaultHttpContext CreateContext(string method, string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        return context;
    }

    private sealed class RecordingRateLimiter : IRateLimiter
    {
        public bool Allow { get; init; } = true;
        public List<(string Key, int Limit)> Attempts { get; } = [];

        public bool TryAllowRequest(string key)
        {
            Attempts.Add((key, 0));
            return Allow;
        }

        public bool TryAllowRequest(string key, int requestsPerMinute)
        {
            Attempts.Add((key, requestsPerMinute));
            return Allow;
        }

        public void SetLimit(int requestsPerMinute)
        {
        }
    }
}
