using HuanyuFlowerShop.Entities;

namespace HuanyuFlowerShop.Tests;

public sealed class ProductStockTests
{
    [Fact]
    public void ReduceStock_UpdatesStockAndSalesCount()
    {
        var product = new Product { Stock = 5, SalesCount = 2 };

        var reduced = product.ReduceStock(3);

        Assert.True(reduced);
        Assert.Equal(2, product.Stock);
        Assert.Equal(5, product.SalesCount);
    }

    [Fact]
    public void ReduceStock_ReturnsFalseWithoutGoingNegative()
    {
        var product = new Product { Stock = 2 };

        var reduced = product.ReduceStock(3);

        Assert.False(reduced);
        Assert.Equal(2, product.Stock);
        Assert.Equal(0, product.SalesCount);
    }
}
