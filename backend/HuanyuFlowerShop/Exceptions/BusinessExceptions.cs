namespace HuanyuFlowerShop.Exceptions;

/// <summary>
/// 业务异常基类
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
    public BusinessException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// 库存不足异常
/// </summary>
public class InsufficientStockException : BusinessException
{
    public string ProductName { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    public InsufficientStockException(string productName, int requestedQuantity, int availableQuantity)
        : base($"产品 {productName} 库存不足。请求：{requestedQuantity}，可用：{availableQuantity}")
    {
        ProductName = productName;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}

/// <summary>
/// 产品不存在异常
/// </summary>
public class ProductNotFoundException : BusinessException
{
    public int ProductId { get; }

    public ProductNotFoundException(int productId)
        : base($"产品ID {productId} 不存在")
    {
        ProductId = productId;
    }
}

/// <summary>
/// 用户不存在异常
/// </summary>
public class UserNotFoundException : BusinessException
{
    public int UserId { get; }

    public UserNotFoundException(int userId)
        : base($"用户ID {userId} 不存在")
    {
        UserId = userId;
    }
}

/// <summary>
/// 订单不存在异常
/// </summary>
public class OrderNotFoundException : BusinessException
{
    public int OrderId { get; }

    public OrderNotFoundException(int orderId)
        : base($"订单ID {orderId} 不存在")
    {
        OrderId = orderId;
    }
}

/// <summary>
/// 无效操作异常
/// </summary>
public class InvalidOperationException : BusinessException
{
    public InvalidOperationException(string message) : base(message) { }
}

/// <summary>
/// 权限不足异常
/// </summary>
public class UnauthorizedException : BusinessException
{
    public UnauthorizedException(string message) : base(message) { }
}