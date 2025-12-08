using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using HuanyuFlowerShop.Services;
using HuanyuFlowerShop.Interfaces;
using Microsoft.Extensions.Logging;
using HuanyuFlowerShop.DTOs;

namespace HuanyuFlowerShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const string USER_ORDER_STATS_CACHE_KEY_PREFIX = "user_order_stats_";
        private readonly IAuthService _authService;
        private readonly IOrderService _orderService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IOrderService orderService, IWebHostEnvironment environment, ILogger<AuthController> logger)
        {
            _authService = authService;
            _orderService = orderService;
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new LoginResponseDto
                {
                    Success = false,
                    Message = "输入数据无效"
                });
            }

            var result = await _authService.LoginAsync(loginDto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new RegisterResponseDto
                {
                    Success = false,
                    Message = "输入数据无效"
                });
            }

            var result = await _authService.RegisterAsync(registerDto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
            {
                return Unauthorized();
            }

            try
            {
                var userDto = await _authService.GetUserByIdAsync(id);
                
                if (userDto == null)
                {
                    return NotFound(new { Success = false, Message = "用户不存在" });
                }

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"获取用户信息失败: {ex.Message}" });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetUserProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
            {
                return Unauthorized();
            }

            try
            {
                var userDto = await _authService.GetUserByIdAsync(id);
                
                if (userDto == null)
                {
                    return NotFound(new { Success = false, Message = "用户不存在" });
                }

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"获取用户信息失败: {ex.Message}" });
            }
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<UpdateUserResponseDto>> UpdateUserProfile([FromBody] UpdateUserDto updateUserDto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _authService.UpdateUserProfileAsync(id, updateUserDto);
                
                if (!result.Success)
                {
                    return BadRequest(new { Success = false, Message = result.Message });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"更新用户信息失败: {ex.Message}" });
            }
        }

        [HttpGet("addresses")]
        [Authorize]
        public async Task<IActionResult> GetAddresses()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
            {
                return Unauthorized();
            }

            try
            {
                var addresses = await _authService.GetAddressesAsync(id);
                return Ok(addresses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取地址列表失败: {ex.Message}" });
            }
        }

        [HttpPost("addresses")]
        [Authorize]
        public async Task<IActionResult> AddAddress([FromBody] HuanyuFlowerShop.DTOs.CreateAddressDto createAddressDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
            {
                return Unauthorized();
            }

            try
            {
                // 转换CreateAddressDto到AddressDto
                var addressDto = new HuanyuFlowerShop.DTOs.AddressDto
                {
                    RecipientName = createAddressDto.RecipientName,
                    PhoneNumber = createAddressDto.PhoneNumber,
                    Province = createAddressDto.Province,
                    City = createAddressDto.City,
                    District = createAddressDto.District,
                    DetailAddress = createAddressDto.DetailAddress,
                    PostalCode = createAddressDto.PostalCode,
                    IsDefault = createAddressDto.IsDefault
                };
                
                var address = await _authService.AddAddressAsync(id, addressDto);
                return Ok(address);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"添加地址失败: {ex.Message}" });
            }
        }

        [HttpPut("addresses/{addressId}")]
        [Authorize]
        public async Task<IActionResult> UpdateAddress(int addressId, [FromBody] HuanyuFlowerShop.DTOs.AddressDto addressDto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
            {
                return Unauthorized();
            }

            try
            {
                var address = await _authService.UpdateAddressAsync(id, addressId, addressDto);
                return Ok(address);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"更新地址失败: {ex.Message}" });
            }
        }

        [HttpDelete("addresses/{addressId}")]
        [Authorize]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _authService.DeleteAddressAsync(id, addressId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"删除地址失败: {ex.Message}" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public ActionResult Logout()
        {
            // 在实际应用中，可以在这里实现token黑名单机制
            // 目前只是返回成功响应
            return Ok(new { Success = true, Message = "退出登录成功" });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<bool>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
                {
                    return Unauthorized();
                }

                var result = await _authService.ChangePasswordAsync(id, changePasswordDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "修改密码时发生错误");
            }
        }

        [HttpGet("order-stats")]
        [Authorize]
        public async Task<ActionResult<object>> GetOrderStats()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized();
                }
                
                _logger.LogInformation("获取用户订单统计信息，用户ID: {UserId}", userId);
                
                // 使用注入的OrderService获取订单统计
                var stats = await _orderService.GetUserOrderStatsAsync(userId);
                
                _logger.LogInformation("获取用户订单统计信息成功，用户ID: {UserId}", userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户订单统计信息失败");
                return StatusCode(500, new { message = "获取订单统计信息失败: " + ex.Message });
            }
        }

        [HttpPost("upload-avatar")]
        [Authorize]
        public async Task<ActionResult<object>> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Success = false, Message = "请选择要上传的图片" });
            }

            // 验证文件类型
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { Success = false, Message = "只支持 JPG、PNG、GIF、WebP 格式的图片" });
            }

            // 验证文件大小（限制为5MB）
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { Success = false, Message = "图片大小不能超过5MB" });
            }

            try
            {
                // 创建上传目录
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 生成唯一文件名
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var uniqueFileName = $"{userId}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 保存文件
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // 更新用户头像URL
                var avatarUrl = $"/uploads/avatars/{uniqueFileName}";
                var updateResult = await _authService.UpdateUserAvatarAsync(int.Parse(userId!), avatarUrl);

                if (!updateResult.Success)
                {
                    // 如果更新数据库失败，删除已上传的文件
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    return BadRequest(new { Success = false, Message = updateResult.Message });
                }

                // 返回完整的URL，与产品图片保持一致
                var fullAvatarUrl = $"{Request.Scheme}://{Request.Host}{avatarUrl}";

                return Ok(new { 
                    Success = true, 
                    Message = "头像上传成功",
                    AvatarUrl = fullAvatarUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "上传失败：" + ex.Message });
            }
        }
    }
}