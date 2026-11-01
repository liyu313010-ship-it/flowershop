using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Services;
using System.Security.Claims;

namespace HuanyuFlowerShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController(IUserService userService, ILogger<ProfileController> logger) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly ILogger<ProfileController> _logger = logger;

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not authenticated");
        }

        /// <summary>
        /// 获取当前用户资料
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // 从JWT token中获取用户ID或用户名
                var userIdString = GetCurrentUserId();
                
                // 尝试将用户ID转换为整数
                if (!int.TryParse(userIdString, out int userId))
                {
                    // 如果不是整数ID，尝试通过用户名获取
                    var user = await _userService.GetUserByUsernameAsync(userIdString);
                    if (user == null)
                    {
                        return NotFound(new { Success = false, Message = "用户不存在" });
                    }
                    return Ok(user);
                }
                
                var profile = await _userService.GetUserByIdAsync(userId);
                if (profile == null)
                {
                    return NotFound(new { Success = false, Message = "用户不存在" });
                }
                
                return Ok(profile);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "未授权的访问尝试");
                return Unauthorized(new { Success = false, ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户资料时发生错误");
                return StatusCode(500, new { Success = false, Message = "获取用户资料失败，请稍后重试" });
            }
        }

        /// <summary>
        /// 更新当前用户资料
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto updateProfileDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 从JWT token中获取用户ID或用户名
                var userIdString = GetCurrentUserId();
                
                // 尝试将用户ID转换为整数
                if (!int.TryParse(userIdString, out int userId))
                {
                    // 如果不是整数ID，尝试通过用户名获取
                    var user = await _userService.GetUserByUsernameAsync(userIdString);
                    if (user == null)
                    {
                        return NotFound(new { Success = false, Message = "用户不存在" });
                    }
                    userId = user.Id;
                }
                
                var updatedUser = await _userService.UpdateUserAsync(userId, updateProfileDto);
                
                return Ok(new UpdateUserResponseDto
                {
                    Success = true,
                    Message = "用户资料更新成功",
                    User = updatedUser
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "用户资料更新失败：{Message}", ex.Message);
                return BadRequest(new UpdateUserResponseDto
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "未授权的更新尝试");
                return Unauthorized(new UpdateUserResponseDto
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户资料时发生错误");
                return StatusCode(500, new UpdateUserResponseDto
                {
                    Success = false,
                    Message = "更新用户资料失败，请稍后重试"
                });
            }
        }

        /// <summary>
        /// 更改用户密码
        /// </summary>
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 从JWT token中获取用户ID或用户名
                var userIdString = GetCurrentUserId();
                
                // 尝试将用户ID转换为整数
                if (!int.TryParse(userIdString, out int userId))
                {
                    // 如果不是整数ID，尝试通过用户名获取
                    var user = await _userService.GetUserByUsernameAsync(userIdString);
                    if (user == null)
                    {
                        return NotFound(new UpdateUserResponseDto
                        { 
                            Success = false, 
                            Message = "用户不存在"
                        });
                    }
                    userId = user.Id;
                }
                
                // 验证当前密码是否正确
                bool isPasswordValid = await _userService.VerifyPasswordAsync(userId, changePasswordDto.CurrentPassword);
                if (!isPasswordValid)
                {
                    return BadRequest(new UpdateUserResponseDto
                    { 
                        Success = false, 
                        Message = "当前密码不正确"
                    });
                }
                
                // 创建更新用户DTO，仅设置密码字段
                var updateUserDto = new UpdateUserDto
                {
                    // 保持其他字段不变，仅更新密码
                    Password = changePasswordDto.NewPassword
                };
                
                // 获取当前用户信息以填充其他字段
                var currentUser = await _userService.GetUserByIdAsync(userId);
                if (currentUser == null)
                {
                    return NotFound(new UpdateUserResponseDto
                    { 
                        Success = false, 
                        Message = "用户不存在"
                    });
                }
                
                // 填充其他必填字段
                updateUserDto.Username = currentUser.Username;
                updateUserDto.Email = currentUser.Email;
                updateUserDto.FullName = currentUser.FullName;
                updateUserDto.Phone = currentUser.Phone;
                updateUserDto.Address = currentUser.Address;
                updateUserDto.Gender = currentUser.Gender;
                updateUserDto.Avatar = currentUser.Avatar;
                
                var updatedUser = await _userService.UpdateUserAsync(userId, updateUserDto);
                
                return Ok(new UpdateUserResponseDto
                { 
                    Success = true, 
                    Message = "密码修改成功",
                    User = updatedUser
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "密码修改失败：{Message}", ex.Message);
                return BadRequest(new UpdateUserResponseDto
                { 
                    Success = false, 
                    Message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "未授权的密码修改尝试");
                return Unauthorized(new UpdateUserResponseDto
                { 
                    Success = false, 
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "修改密码时发生错误");
                return StatusCode(500, new UpdateUserResponseDto
                { 
                    Success = false, 
                    Message = "修改密码失败，请稍后重试"
                });
            }
        }
    }
    
    // ChangePasswordDto已在AuthDto.cs中定义
}