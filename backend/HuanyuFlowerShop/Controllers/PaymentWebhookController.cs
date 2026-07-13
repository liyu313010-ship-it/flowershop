using System.Security.Cryptography;
using System.Text;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HuanyuFlowerShop.Controllers;

[ApiController]
[Route("api/payment")]
public sealed class PaymentWebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentWebhookController> _logger;

    public PaymentWebhookController(IConfiguration configuration, IPaymentService paymentService, ILogger<PaymentWebhookController> logger)
    {
        _configuration = configuration;
        _paymentService = paymentService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Receive([FromBody] PaymentWebhookRequest request)
    {
        var secret = _configuration["Payment:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(secret)) return NotFound();
        if (request.OrderId <= 0 || request.Amount <= 0 || string.IsNullOrWhiteSpace(request.PaymentReference) || request.Timestamp <= 0)
            return BadRequest(new { message = "支付回调参数不完整" });
        if (Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - request.Timestamp) > 300)
            return BadRequest(new { message = "支付回调已过期" });

        var payload = $"{request.OrderId}|{request.Amount:0.00}|{request.PaymentReference}|{request.Timestamp}|{request.Status}";
        var expected = Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(request.Signature.Trim().ToLowerInvariant())))
            return Unauthorized(new { message = "支付回调签名无效" });

        if (!string.Equals(request.Status, "paid", StringComparison.OrdinalIgnoreCase))
            return Ok(new { success = true, ignored = true });

        var result = await _paymentService.UpdatePaymentStatusAsync(request.OrderId, new PaymentStatusRequest
        {
            PaymentStatus = "paid",
            PaymentReference = request.PaymentReference.Trim(),
            PaymentMethod = request.PaymentMethod,
            Amount = request.Amount
        });
        if (!result.Success)
        {
            _logger.LogWarning("支付回调处理失败：订单 {OrderId}，原因：{Message}", request.OrderId, result.Message);
            return BadRequest(new { message = result.Message });
        }
        return Ok(new { success = true });
    }
}

public sealed class PaymentWebhookRequest
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "online";
    public string Status { get; set; } = string.Empty;
    public long Timestamp { get; set; }
    public string Signature { get; set; } = string.Empty;
}
