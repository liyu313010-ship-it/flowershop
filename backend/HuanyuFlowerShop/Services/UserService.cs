using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.DTOs;
using BCrypt.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace HuanyuFlowerShop.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetUserByUsernameAsync(string username);
        Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UpdateUserStatusAsync(int id, string status);
        Task<bool> VerifyPasswordAsync(int userId, string password);
    }

    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UserService> _logger;

        // 缓存键常量
        private const string ALL_USERS_CACHE_KEY = "all_users";
        private const string USER_BY_ID_CACHE_KEY_PREFIX = "user_id_";
        private const string USER_BY_USERNAME_CACHE_KEY_PREFIX = "user_username_";

        public UserService(IRepository<User> userRepository, ICacheService cacheService, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            _logger.LogInformation("开始获取所有用户信息");
            
            // 尝试从缓存获取
            var cachedUsers = await _cacheService.GetAsync<IEnumerable<UserDto>>(ALL_USERS_CACHE_KEY);
            if (cachedUsers != null)
            {
                _logger.LogInformation("从缓存获取所有用户信息成功");
                return cachedUsers;
            }
            
            _logger.LogInformation("缓存未命中，从数据库获取所有用户信息");
            var users = await _userRepository.GetAllAsync();
            var result = users.Select(user => new UserDto
            {
                Id = user.Id,
                Username = user.Username ?? "",
                Email = user.Email ?? "",
                Role = user.Role ?? "user",
                Status = user.Status ?? "unknown",
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt
            });
            
            // 缓存结果，有效期15分钟
            await _cacheService.SetAsync(ALL_USERS_CACHE_KEY, result, TimeSpan.FromMinutes(15));
            _logger.LogInformation("获取所有用户信息完成");
            
            return result;
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("用户ID必须大于0", nameof(id));
            }
            
            _logger.LogInformation("开始获取用户信息，用户ID: {UserId}", id);
            
            // 尝试从缓存获取
            string cacheKey = $"{USER_BY_ID_CACHE_KEY_PREFIX}{id}";
            var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation("从缓存获取用户信息成功，用户ID: {UserId}", id);
                return cachedUser;
            }
            
            _logger.LogInformation("缓存未命中，从数据库获取用户信息，用户ID: {UserId}", id);
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogInformation("用户不存在，用户ID: {UserId}", id);
                return null;
            }

            var result = new UserDto
            {
                Id = user.Id,
                Username = user.Username ?? "",
                Email = user.Email ?? "",
                FullName = user.FullName ?? "",
                Phone = user.Phone ?? "",
                Address = user.Address ?? "",
                Gender = user.Gender ?? "",
                Role = user.Role ?? "user",
                Status = user.Status ?? "unknown",
                Avatar = user.Avatar ?? "",
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt
            };
            
            // 缓存结果，有效期30分钟
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
            _logger.LogInformation("获取用户信息完成，用户ID: {UserId}", id);
            
            return result;
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("用户名不能为空", nameof(username));
            }
            
            _logger.LogInformation("开始获取用户信息，用户名: {Username}", username);
            
            // 尝试从缓存获取
            string cacheKey = $"{USER_BY_USERNAME_CACHE_KEY_PREFIX}{username}";
            var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation("从缓存获取用户信息成功，用户名: {Username}", username);
                return cachedUser;
            }
            
            _logger.LogInformation("缓存未命中，从数据库获取用户信息，用户名: {Username}", username);
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username == username);
            
            if (user == null)
            {
                _logger.LogInformation("用户不存在，用户名: {Username}", username);
                return null;
            }

            var result = new UserDto
            {
                Id = user.Id,
                Username = user.Username ?? "",
                Email = user.Email ?? "",
                Role = user.Role ?? "user",
                Status = user.Status ?? "unknown",
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt
            };
            
            // 缓存结果，有效期30分钟
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
            _logger.LogInformation("获取用户信息完成，用户名: {Username}", username);
            
            return result;
        }

        public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            if (id <= 0)
            {
                throw new ArgumentException("用户ID必须大于0", nameof(id));
            }
            
            if (updateUserDto == null)
            {
                throw new ArgumentNullException(nameof(updateUserDto), "更新数据不能为空");
            }
            
            _logger.LogInformation("开始更新用户信息，用户ID: {UserId}", id);
            
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("用户不存在，用户ID: {UserId}", id);
                throw new ArgumentException("用户不存在");
            }

            // 检查用户名是否已存在（除了当前用户）
            var users = await _userRepository.GetAllAsync();
            var existingUser = users.FirstOrDefault(u => u.Username == updateUserDto.Username && u.Id != id);
            if (existingUser != null)
            {
                throw new ArgumentException("用户名已存在");
            }

            // 检查邮箱是否已存在（除了当前用户）
            existingUser = users.FirstOrDefault(u => u.Email == updateUserDto.Email && u.Id != id);
            if (existingUser != null)
            {
                throw new ArgumentException("邮箱已存在");
            }

            user.Username = updateUserDto.Username;
            user.Email = updateUserDto.Email;
            
            // 更新其他字段
            if (!string.IsNullOrEmpty(updateUserDto.FullName))
            {
                user.FullName = updateUserDto.FullName;
            }
            
            if (!string.IsNullOrEmpty(updateUserDto.Phone))
            {
                user.Phone = updateUserDto.Phone;
            }
            
            if (!string.IsNullOrEmpty(updateUserDto.Address))
            {
                user.Address = updateUserDto.Address;
            }
            
            if (!string.IsNullOrEmpty(updateUserDto.Gender))
            {
                user.Gender = updateUserDto.Gender;
            }
            
            if (!string.IsNullOrEmpty(updateUserDto.Avatar))
            {
                user.Avatar = updateUserDto.Avatar;
            }

            // 如果提供了新密码，则更新密码
            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            
            // 清除缓存
            await ClearUserCache(id, user.Username);
            
            _logger.LogInformation("更新用户信息完成，用户ID: {UserId}", id);
            
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username ?? "",
                Email = user.Email ?? "",
                Role = user.Role ?? "user",
                Status = user.Status ?? "unknown",
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("用户ID必须大于0", nameof(id));
            }
            
            _logger.LogInformation("开始删除用户，用户ID: {UserId}", id);
            
            // 先获取用户信息以便清除缓存
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("用户不存在，用户ID: {UserId}", id);
                return false;
            }
            
            var result = await _userRepository.DeleteAsync(id);
            
            if (result)
            {
                // 清除缓存
                await ClearUserCache(id, user.Username);
                _logger.LogInformation("删除用户成功，用户ID: {UserId}", id);
            }
            else
            {
                _logger.LogError("删除用户失败，用户ID: {UserId}", id);
            }
            
            return result;
        }

        public async Task<bool> UpdateUserStatusAsync(int id, string status)
        {
            if (id <= 0)
            {
                throw new ArgumentException("用户ID必须大于0", nameof(id));
            }
            
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException("用户状态不能为空", nameof(status));
            }
            
            _logger.LogInformation("开始更新用户状态，用户ID: {UserId}, 新状态: {Status}", id, status);
            
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("用户不存在，用户ID: {UserId}", id);
                return false;
            }

            var validStatuses = new[] { "active", "inactive", "banned" };
            if (!validStatuses.Contains(status))
            {
                throw new ArgumentException("无效的用户状态");
            }

            user.Status = status;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userRepository.UpdateAsync(user);
            
            if (result)
            {
                // 清除缓存
                await ClearUserCache(id, user.Username);
                _logger.LogInformation("更新用户状态成功，用户ID: {UserId}, 新状态: {Status}", id, status);
            }
            else
            {
                _logger.LogError("更新用户状态失败，用户ID: {UserId}", id);
            }
            
            return result;
        }

        public async Task<bool> VerifyPasswordAsync(int userId, string password)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("用户ID必须大于0", nameof(userId));
            }
            
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("密码不能为空", nameof(password));
            }
            
            _logger.LogInformation("开始验证用户密码，用户ID: {UserId}", userId);
            
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("用户不存在，用户ID: {UserId}", userId);
                return false;
            }
            
            // 使用BCrypt验证密码
            var result = BCrypt.Net.BCrypt.Verify(password, user.Password);
            _logger.LogInformation("密码验证{Result}，用户ID: {UserId}", result ? "成功" : "失败", userId);
            
            return result;
        }
        
        /// <summary>
        /// 清除用户相关缓存
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="username">用户名</param>
        private async Task ClearUserCache(int userId, string username)
        {
            try
            {
                var cacheKeys = new List<string>
                {
                    ALL_USERS_CACHE_KEY,
                    $"{USER_BY_ID_CACHE_KEY_PREFIX}{userId}"
                };
                
                if (!string.IsNullOrEmpty(username))
                {
                    cacheKeys.Add($"{USER_BY_USERNAME_CACHE_KEY_PREFIX}{username}");
                }
                
                foreach (var key in cacheKeys)
                {
                    await _cacheService.RemoveAsync(key);
                }
                
                _logger.LogInformation("清除用户缓存成功，用户ID: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除用户缓存失败，用户ID: {UserId}", userId);
            }
        }
    }
}