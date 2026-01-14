using Microsoft.AspNetCore.Mvc;
using HuanyuFlowerShop.Interfaces;
using Aop.Api.Util;

namespace HuanyuFlowerShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentCallbackController(
        IPaymentService paymentService,
        ILogger<PaymentCallbackController> logger,
        IConfiguration configuration) : ControllerBase
    {
        private readonly IPaymentService _paymentService = paymentService;
        private readonly ILogger<PaymentCallbackController> _logger = logger;
        private readonly IConfiguration _configuration = configuration;

        [HttpPost("notify")]
        public IActionResult Notify()
        {
            try
            {
                var dict = new Dictionary<string, string>();
                var keys = Request.Form.Keys;
                foreach (var key in keys)
                {
                    dict.Add(key, Request.Form[key].ToString() ?? string.Empty);
                }

                _logger.LogInformation("收到支付宝异步通知: {@Dict}", dict);

                if (dict.Count == 0)
                {
                    return BadRequest("无参数");
                }

                var config = _configuration.GetSection("Alipay");
                var payMode = _configuration.GetSection("Payment")["Mode"];
                
                bool signVerified = false;
                if (string.Equals(payMode, "mock", StringComparison.OrdinalIgnoreCase))
                {
                     signVerified = true;
                }
                else
                {
                    signVerified = AlipaySignature.RSACheckV1(
                        dict, 
                        config["AlipayPublicKey"], 
                        "utf-8", 
                        config["SignType"], 
                        false);
                }

                if (signVerified)
                {
                    string tradeStatus = dict["trade_status"];
                    string outTradeNo = dict["out_trade_no"];
                    var r = _paymentService.ProcessCallbackAsync(outTradeNo, tradeStatus, "alipay").GetAwaiter().GetResult();
                    if (!r.Success) _logger.LogWarning("支付回调处理失败: {Message}", r.Message);

                    return Content("success");
                }
                else
                {
                    _logger.LogWarning("支付宝验签失败");
                    return Content("fail");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理支付宝回调失败");
                return Content("fail");
            }
        }
    }
}
