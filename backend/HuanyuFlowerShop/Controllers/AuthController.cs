using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using HuanyuFlowerShop.Services;
using HuanyuFlowerShop.Interfaces;
using Microsoft.Extensions.Logging;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using HuanyuFlowerShop.Options;
using Microsoft.Extensions.Options;

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
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _email;
        private readonly StorageOptions _storageOptions;

        public AuthController(IAuthService authService, IOrderService orderService, IWebHostEnvironment environment, ILogger<AuthController> logger, ApplicationDbContext db, IEmailService email, IOptions<StorageOptions> storageOptions)
        {
            _authService = authService;
            _orderService = orderService;
            _environment = environment;
            _logger = logger;
            _db = db;
            _email = email;
            _storageOptions = storageOptions.Value;
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "邮箱格式不正确" });
            var generic = new { message = "如果该邮箱已注册，密码重置邮件将在几分钟内送达" };
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email && u.Status == "active");
            if (user == null || !_email.IsEnabled) return Ok(generic);

            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
            var oldTokens = await _db.PasswordResetTokens.Where(t => t.UserId == user.Id && t.UsedAt == null).ToListAsync();
            foreach (var old in oldTokens) old.UsedAt = DateTime.UtcNow;
            _db.PasswordResetTokens.Add(new PasswordResetToken { UserId = user.Id, TokenHash = hash, ExpiresAt = DateTime.UtcNow.AddMinutes(30) });
            await _db.SaveChangesAsync();
            var resetUrl = $"{Request.Scheme}://{Request.Host}/auth?resetToken={Uri.EscapeDataString(rawToken)}";
            await _email.SendAsync(user.Email, "欢雨鲜花密码重置", $"您好，您的密码重置链接（30分钟内有效）：\n{resetUrl}\n如果不是您本人操作，请忽略此邮件。", HttpContext.RequestAborted);
            return Ok(generic);
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "密码格式不正确" });
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Token.Trim())));
            var token = await _db.PasswordResetTokens.Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == hash && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow);
            if (token?.User == null) return BadRequest(new { message = "重置链接无效或已过期" });
            token.User.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            token.User.UpdatedAt = DateTime.UtcNow;
            token.UsedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "密码重置成功，请重新登录" });
        }

        [Authorize]
        [HttpPost("send-verification")]
        public async Task<IActionResult> SendVerification()
        {
            var userId = GetCurrentUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user is null) return NotFound(new { message = "用户不存在" });
            if (user.EmailVerified) return Ok(new { success = true, message = "邮箱已验证" });
            if (!_email.IsEnabled) return Conflict(new { message = "邮件服务暂未配置" });

            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
            var oldTokens = await _db.EmailVerificationTokens.Where(t => t.UserId == user.Id && t.UsedAt == null).ToListAsync();
            foreach (var old in oldTokens) old.UsedAt = DateTime.UtcNow;
            _db.EmailVerificationTokens.Add(new EmailVerificationToken { UserId = user.Id, TokenHash = hash, ExpiresAt = DateTime.UtcNow.AddHours(24) });
            await _db.SaveChangesAsync();
            var verifyUrl = $"{Request.Scheme}://{Request.Host}/auth?verifyToken={Uri.EscapeDataString(rawToken)}";
            await _email.SendAsync(user.Email, "欢雨鲜花邮箱验证", $"您好，请在24小时内点击链接验证邮箱：\n{verifyUrl}", HttpContext.RequestAborted);
            return Ok(new { success = true, message = "验证邮件已发送" });
        }

        [AllowAnonymous]
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Token)) return BadRequest(new { message = "验证令牌不能为空" });
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Token.Trim())));
            var token = await _db.EmailVerificationTokens.Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == hash && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow);
            if (token?.User is null) return BadRequest(new { message = "验证链接无效或已过期" });
            token.User.EmailVerified = true;
            token.User.UpdatedAt = DateTime.UtcNow;
            token.UsedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "邮箱验证成功" });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = GetCurrentUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user is null) return NotFound(new { message = "用户不存在" });
            user.TokenVersion++;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "已退出登录" });
        }

        private int GetCurrentUserId()
        {
            var value = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var id) && id > 0 ? id : throw new UnauthorizedAccessException("登录状态已失效");
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
                return result.Message == "登录服务暂不可用，请稍后重试"
                    ? StatusCode(StatusCodes.Status503ServiceUnavailable, result)
                    : Unauthorized(result);
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

            await using (var input = file.OpenReadStream())
            {
                var header = new byte[12];
                var read = await input.ReadAsync(header.AsMemory(0, header.Length));
                var validHeader = fileExtension switch
                {
                    ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
                    ".png" => read >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
                    ".gif" => read >= 6 && (header.AsSpan(0, 6).SequenceEqual("GIF87a"u8) || header.AsSpan(0, 6).SequenceEqual("GIF89a"u8)),
                    ".webp" => read >= 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
                    _ => false
                };
                if (!validHeader) return BadRequest(new { Success = false, Message = "文件内容不是有效的图片" });
            }

            try
            {
                // 创建上传目录
                var rootPath = Path.IsPathRooted(_storageOptions.RootPath)
                    ? _storageOptions.RootPath
                    : Path.Combine(_environment.ContentRootPath, _storageOptions.RootPath);
                var uploadsFolder = Path.Combine(rootPath, "avatars");
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

public sealed class VerifyEmailDto
{
    public string Token { get; set; } = string.Empty;
}
