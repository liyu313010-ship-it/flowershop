using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HuanyuFlowerShop.Entities;

using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Data;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace HuanyuFlowerShop.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AuthService> _logger;

        // 缓存键常量
        private const string USER_BY_ID_CACHE_KEY_PREFIX = "user_id_";
        private const string USER_ADDRESSES_CACHE_KEY_PREFIX = "user_addresses_";

        public AuthService(IRepository<User> userRepository, ApplicationDbContext dbContext, IConfiguration configuration, 
                          ICacheService cacheService, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _dbContext = dbContext;
            _configuration = configuration;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                if (loginDto == null || string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
                {
                    throw new ArgumentException("登录信息不完整");
                }
                
                _logger.LogInformation("开始用户登录验证，用户名: {Username}", loginDto.Username);
                
                var users = await _userRepository.GetAllAsync();
                var user = users.FirstOrDefault(u => u.Username == loginDto.Username);

                if (user == null || string.IsNullOrEmpty(user.Username))
                {
                    _logger.LogWarning("用户不存在，用户名: {Username}", loginDto.Username);
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "用户名或密码错误"
                    };
                }

                _logger.LogInformation("找到用户，用户ID: {UserId}, 密码哈希: {PasswordHash}", user.Id, user.Password);
                _logger.LogInformation("登录密码: {Password}", loginDto.Password);

                // 使用BCrypt验证密码
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password);
                _logger.LogInformation("密码验证结果: {IsValid}", isPasswordValid);

                if (!isPasswordValid)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "用户名或密码错误"
                    };
                }

                if (user.Status != "active")
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "你的账户被封禁，请联系管理员"
                    };
                }

                // 更新上次登录时间
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                // 清空用户缓存，确保下次获取用户信息时能从数据库中获取最新数据
                await ClearUserCache(user.Id, user.Username);

                var token = GenerateJwtToken(user);
                
                _logger.LogInformation("用户登录成功，用户ID: {UserId}", user.Id);

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "登录成功",
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username ?? "",
                        Email = user.Email ?? "",
                        FullName = user.FullName ?? "",
                        Phone = user.Phone ?? "",
                        Address = user.Address ?? "",
                        Role = user.Role ?? "user",
                        Status = user.Status ?? "unknown",
                        Avatar = user.Avatar ?? "",
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt,
                        LastLoginAt = user.LastLoginAt
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户登录失败: {Username}", loginDto?.Username);
                return new LoginResponseDto
                {
                    Success = false,
                    Message = $"登录失败: {ex.Message}"
                };
            }
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                if (registerDto == null || string.IsNullOrEmpty(registerDto.Username) || string.IsNullOrEmpty(registerDto.Password) || string.IsNullOrEmpty(registerDto.Email))
                {
                    _logger.LogWarning("注册信息验证失败：缺少必填字段");
                    throw new ArgumentException("注册信息不完整");
                }
                
                _logger.LogInformation("开始用户注册流程，用户名: {Username}, 邮箱: {Email}", registerDto.Username, registerDto.Email);
                
                _logger.LogDebug("正在从数据库获取所有用户信息以检查唯一性");
                var users = await _userRepository.GetAllAsync();
                _logger.LogDebug("成功获取到用户列表，共 {UserCount} 个用户", users.Count());
                
                // 检查用户名是否已存在
                _logger.LogDebug("检查用户名 {Username} 是否已存在", registerDto.Username);
                if (users.Any(u => u.Username == registerDto.Username))
                {
                    _logger.LogInformation("用户名 {Username} 已被占用", registerDto.Username);
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = "用户名已存在"
                    };
                }

                // 检查邮箱是否已存在
                _logger.LogDebug("检查邮箱 {Email} 是否已存在", registerDto.Email);
                if (users.Any(u => u.Email == registerDto.Email))
                {
                    _logger.LogInformation("邮箱 {Email} 已被占用", registerDto.Email);
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = "邮箱已存在"
                    };
                }

                _logger.LogDebug("开始创建新用户对象");
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password), // 使用BCrypt加密密码
                    FullName = registerDto.FullName,
                    Phone = registerDto.Phone,
                    Address = registerDto.Address,
                    Role = "user",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                };
                _logger.LogDebug("用户对象创建完成，即将保存到数据库");

                _logger.LogInformation("正在将用户 {Username} 保存到数据库", registerDto.Username);
                var createdUser = await _userRepository.AddAsync(user);
                _logger.LogDebug("数据库保存操作完成");

                if (createdUser == null || createdUser.Id <= 0)
                {
                    _logger.LogError("用户创建失败：数据库返回无效用户对象或ID");
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = "注册失败：无法创建用户"
                    };
                }

                _logger.LogInformation("用户 {Username} 注册成功，生成的用户ID: {UserId}", createdUser.Username, createdUser.Id);
                
                return new RegisterResponseDto
                {
                    Success = true,
                    Message = "注册成功",
                    User = new UserDto
                        {
                            Id = createdUser.Id,
                            Username = createdUser.Username ?? "",
                            Email = createdUser.Email ?? "",
                            FullName = createdUser.FullName ?? "",
                            Phone = createdUser.Phone ?? "",
                            Address = createdUser.Address ?? "",
                            Role = createdUser.Role ?? "user",
                            Status = createdUser.Status ?? "unknown",
                            Avatar = createdUser.Avatar ?? "",
                            CreatedAt = createdUser.CreatedAt,
                            UpdatedAt = createdUser.UpdatedAt,
                            LastLoginAt = createdUser.LastLoginAt
                        }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户注册过程中发生异常: 用户名={Username}, 错误信息={ErrorMessage}", 
                    registerDto?.Username, ex.Message);
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = $"注册失败: {ex.Message}"
                };
            }
        }

        public string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Key is not configured");
            var jwtIssuer = _configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
            var jwtAudience = _configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "user"),
                new Claim("status", user.Status ?? "unknown")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<UpdateUserResponseDto> UpdateUserAvatarAsync(int userId, string avatarUrl)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("用户ID必须大于0");
                }
                
                _logger.LogInformation("开始更新用户头像，用户ID: {UserId}", userId);
                
                var users = await _userRepository.GetAllAsync();
                var user = users.FirstOrDefault(u => u.Id == userId);

                if (user == null)
                {
                    return new UpdateUserResponseDto
                    {
                        Success = false,
                        Message = "用户不存在"
                    };
                }

                user.Avatar = avatarUrl;
                user.UpdatedAt = DateTime.UtcNow;

                    var updateResult = await _userRepository.UpdateAsync(user);
                    
                    // 清除缓存
                    if (updateResult)
                    {
                        await ClearUserCache(userId, user.Username);
                        _logger.LogInformation("更新用户头像成功，用户ID: {UserId}", userId);
                    }

                if (!updateResult)
                {
                    return new UpdateUserResponseDto
                    {
                        Success = false,
                        Message = "更新头像失败"
                    };
                }

                return new UpdateUserResponseDto
                {
                    Success = true,
                    Message = "头像更新成功",
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username ?? "",
                        Email = user.Email ?? "",
                        FullName = user.FullName ?? "",
                        Phone = user.Phone ?? "",
                        Address = user.Address ?? "",
                        Role = user.Role ?? "user",
                        Status = user.Status ?? "unknown",
                        Avatar = user.Avatar ?? "",
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt,
                        LastLoginAt = user.LastLoginAt
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户头像失败，用户ID: {UserId}", userId);
                return new UpdateUserResponseDto
                {
                    Success = false,
                    Message = $"更新头像失败: {ex.Message}"
                };
            }
        }

        public async Task<UpdateUserResponseDto> UpdateUserProfileAsync(int userId, UpdateUserDto updateUserDto)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("用户ID必须大于0");
                }
                
                if (updateUserDto == null)
                {
                    throw new ArgumentNullException(nameof(updateUserDto), "更新数据不能为空");
                }
                
                _logger.LogInformation("开始更新用户资料，用户ID: {UserId}", userId);
                
                var users = await _userRepository.GetAllAsync();
                var user = users.FirstOrDefault(u => u.Id == userId);

                if (user == null)
                {
                    return new UpdateUserResponseDto
                    {
                        Success = false,
                        Message = "用户不存在"
                    };
                }

                // 检查用户名是否已被其他用户使用
                if (!string.IsNullOrEmpty(updateUserDto.Username))
                {
                    var existingUser = users.FirstOrDefault(u => u.Username == updateUserDto.Username && u.Id != userId);
                    if (existingUser != null)
                    {
                        return new UpdateUserResponseDto
                        {
                            Success = false,
                            Message = "用户名已被使用"
                        };
                    }
                    user.Username = updateUserDto.Username;
                }

                // 检查邮箱是否已被其他用户使用
                if (!string.IsNullOrEmpty(updateUserDto.Email))
                {
                    var existingUser = users.FirstOrDefault(u => u.Email == updateUserDto.Email && u.Id != userId);
                    if (existingUser != null)
                    {
                        return new UpdateUserResponseDto
                        {
                            Success = false,
                            Message = "邮箱已被使用"
                        };
                    }
                    user.Email = updateUserDto.Email;
                }

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

                // 更新性别信息（统一存中文：男/女/其他）
                if (!string.IsNullOrEmpty(updateUserDto.Gender))
                {
                    var g = updateUserDto.Gender.Trim();
                    var gl = g.ToLowerInvariant();
                    user.Gender = gl switch
                    {
                        "male" => "男",
                        "female" => "女",
                        "other" => "其他",
                        _ => (g == "男" || g == "女" || g == "其他") ? g : g
                    };
                }

                // 更新头像信息
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

                var updateResult = await _userRepository.UpdateAsync(user);
                
                // 清除缓存
                if (updateResult)
                {
                    await ClearUserCache(userId, user.Username);
                    _logger.LogInformation("更新用户资料成功，用户ID: {UserId}", userId);
                }

                if (!updateResult)
                {
                    return new UpdateUserResponseDto
                    {
                        Success = false,
                        Message = "更新用户信息失败"
                    };
                }

                return new UpdateUserResponseDto
                {
                    Success = true,
                    Message = "用户信息更新成功",
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username ?? "",
                        Email = user.Email ?? "",
                        FullName = user.FullName ?? "",
                        Phone = user.Phone ?? "",
                        Address = user.Address ?? "",
                        Role = user.Role ?? "user",
                        Status = user.Status ?? "unknown",
                        Avatar = user.Avatar ?? "",
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt,
                        LastLoginAt = user.LastLoginAt
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户资料失败，用户ID: {UserId}", userId);
                return new UpdateUserResponseDto
                {
                    Success = false,
                    Message = $"更新用户信息失败: {ex.Message}"
                };
            }
        }
        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("用户ID必须大于0");
                }
                
                _logger.LogInformation("开始获取用户信息，用户ID: {UserId}", userId);
                
                // 尝试从缓存获取
                string cacheKey = $"{USER_BY_ID_CACHE_KEY_PREFIX}{userId}";
                var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey);
                if (cachedUser != null)
                {
                    _logger.LogInformation("从缓存获取用户信息成功，用户ID: {UserId}", userId);
                    return cachedUser;
                }
                
                _logger.LogInformation("缓存未命中，从数据库获取用户信息，用户ID: {UserId}", userId);
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogInformation("用户不存在，用户ID: {UserId}", userId);
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
                _logger.LogInformation("获取用户信息完成，用户ID: {UserId}", userId);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户信息失败，用户ID: {UserId}", userId);
                return null;
            }
        }

        public async Task<IEnumerable<AddressDto>> GetAddressesAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("用户ID必须大于0");
            }
            
            _logger.LogInformation("开始获取用户地址列表，用户ID: {UserId}", userId);
            
            // 尝试从缓存获取
            string cacheKey = $"{USER_ADDRESSES_CACHE_KEY_PREFIX}{userId}";
            var cachedAddresses = await _cacheService.GetAsync<IEnumerable<AddressDto>>(cacheKey);
            if (cachedAddresses != null)
            {
                _logger.LogInformation("从缓存获取用户地址列表成功，用户ID: {UserId}", userId);
                return cachedAddresses;
            }
            
            _logger.LogInformation("缓存未命中，从数据库获取用户地址列表，用户ID: {UserId}", userId);
            try
            {
                var addresses = await _dbContext.Addresses
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.CreatedAt)
                    .ToListAsync();

                if (addresses.Any())
                {
                    var result = addresses.Select(a => new AddressDto
                    {
                        Id = a.Id,
                        RecipientName = a.RecipientName ?? "",
                        PhoneNumber = a.PhoneNumber ?? "",
                        Province = a.Province ?? "",
                        City = a.City ?? "",
                        District = a.District ?? "",
                        DetailAddress = a.DetailAddress ?? "",
                        IsDefault = a.IsDefault
                    });

                    await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));
                    _logger.LogInformation("获取用户地址列表完成，用户ID: {UserId}", userId);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "查询地址列表失败，尝试回退到用户表的 Address 字段，用户ID: {UserId}", userId);
            }

            var fallback = await BuildDtoFromUserAddress(userId);
            if (fallback != null)
            {
                var list = new List<AddressDto> { fallback };
                await _cacheService.SetAsync(cacheKey, list, TimeSpan.FromMinutes(10));
                return list;
            }
            return Enumerable.Empty<AddressDto>();
        }

        // 生成完整地址字符串的辅助方法
        private string GenerateFullAddress(Address address)
        {
            return $"{address.Province ?? ""}{address.City ?? ""}{address.District ?? ""}{address.DetailAddress ?? ""} | {address.RecipientName ?? ""} | {address.PhoneNumber ?? ""}";
        }

        // 同步默认地址到User表
        private async Task SyncDefaultAddressToUser(int userId)
        {
            _logger.LogInformation("开始同步默认地址到用户表，用户ID: {UserId}", userId);
            
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                var defaultAddress = await _dbContext.Addresses
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);
                
                if (defaultAddress != null)
                {
                    user.Address = GenerateFullAddress(defaultAddress);
                    await _dbContext.SaveChangesAsync();
                    
                    // 清除用户缓存
                    await ClearUserCache(userId, user.Username);
                    _logger.LogInformation("同步默认地址到用户表成功，用户ID: {UserId}", userId);
                }
            }
        }

        public async Task<AddressDto> AddAddressAsync(int userId, AddressDto addressDto)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("用户ID必须大于0");
            }
            
            if (addressDto == null)
            {
                throw new ArgumentNullException(nameof(addressDto), "地址信息不能为空");
            }
            
            _logger.LogInformation("开始添加用户地址，用户ID: {UserId}", userId);
            // 如果设置为默认地址，先将用户其他地址设置为非默认
            if (addressDto.IsDefault)
            {
                var userAddresses = await _dbContext.Addresses.Where(a => a.UserId == userId).ToListAsync();
                foreach (var address in userAddresses)
                {
                    address.IsDefault = false;
                }
            }

            var newAddress = new Address
            {
                UserId = userId,
                RecipientName = addressDto.RecipientName ?? string.Empty,
                PhoneNumber = addressDto.PhoneNumber ?? string.Empty,
                Province = addressDto.Province ?? string.Empty,
                City = addressDto.City ?? string.Empty,
                District = addressDto.District ?? string.Empty,
                DetailAddress = addressDto.DetailAddress ?? string.Empty,
                PostalCode = addressDto.PostalCode, // 保持可空
                IsDefault = addressDto.IsDefault,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.Addresses.AddAsync(newAddress);
            await _dbContext.SaveChangesAsync();

            // 清除地址缓存
            await ClearUserAddressesCache(userId);
            
            // 如果是默认地址，同步到User表
            if (newAddress.IsDefault)
            {
                await SyncDefaultAddressToUser(userId);
            }
            
            _logger.LogInformation("添加用户地址成功，用户ID: {UserId}, 地址ID: {AddressId}", userId, newAddress.Id);

            return new AddressDto
            {
                Id = newAddress.Id,
                RecipientName = newAddress.RecipientName ?? "",
                PhoneNumber = newAddress.PhoneNumber ?? "",
                Province = newAddress.Province ?? "",
                City = newAddress.City ?? "",
                District = newAddress.District ?? "",
                DetailAddress = newAddress.DetailAddress ?? "",
                IsDefault = newAddress.IsDefault
            };
        }

        public async Task<AddressDto> UpdateAddressAsync(int userId, int addressId, AddressDto addressDto)
        {
            if (userId <= 0 || addressId <= 0)
            {
                throw new ArgumentException("用户ID或地址ID必须大于0");
            }
            
            if (addressDto == null)
            {
                throw new ArgumentNullException(nameof(addressDto), "地址信息不能为空");
            }
            
            _logger.LogInformation("开始更新用户地址，用户ID: {UserId}, 地址ID: {AddressId}", userId, addressId);
            var address = await _dbContext.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
            if (address == null)
            {
                throw new Exception("地址不存在");
            }

            // 如果设置为默认地址，先将用户其他地址设置为非默认
            if (addressDto.IsDefault)
            {
                var userAddresses = await _dbContext.Addresses.Where(a => a.UserId == userId && a.Id != addressId).ToListAsync();
                foreach (var addr in userAddresses)
                {
                    addr.IsDefault = false;
                }
            }

            // 更新地址信息
            address.RecipientName = addressDto.RecipientName ?? string.Empty;
            address.PhoneNumber = addressDto.PhoneNumber ?? string.Empty;
            address.Province = addressDto.Province ?? string.Empty;
            address.City = addressDto.City ?? string.Empty;
            address.District = addressDto.District ?? string.Empty;
            address.DetailAddress = addressDto.DetailAddress ?? string.Empty;
            address.PostalCode = addressDto.PostalCode; // 保持可空
            address.IsDefault = addressDto.IsDefault;
            address.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            
            // 清除地址缓存
            await ClearUserAddressesCache(userId);

            // 如果是默认地址或者之前是默认地址被修改，同步到User表
            if (address.IsDefault || (address.IsDefault != addressDto.IsDefault))
            {
                await SyncDefaultAddressToUser(userId);
            }
            
            _logger.LogInformation("更新用户地址成功，用户ID: {UserId}, 地址ID: {AddressId}", userId, addressId);

            return new AddressDto
            {
                Id = address.Id,
                RecipientName = address.RecipientName ?? "",
                PhoneNumber = address.PhoneNumber ?? "",
                Province = address.Province ?? "",
                City = address.City ?? "",
                District = address.District ?? "",
                DetailAddress = address.DetailAddress ?? "",
                IsDefault = address.IsDefault
            };
        }

        public async Task<bool> DeleteAddressAsync(int userId, int addressId)
        {            
            if (userId <= 0 || addressId <= 0)
            {
                throw new ArgumentException("用户ID或地址ID必须大于0");
            }
            
            _logger.LogInformation("开始删除用户地址，用户ID: {UserId}, 地址ID: {AddressId}", userId, addressId);
            var address = await _dbContext.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
            if (address == null)
            {
                throw new Exception("地址不存在");
            }

            // 记录是否是默认地址
            bool wasDefault = address.IsDefault;

            _dbContext.Addresses.Remove(address);
            await _dbContext.SaveChangesAsync();

            // 如果删除的是默认地址，需要重新同步User表
            if (wasDefault)
            {
                // 查找下一个默认地址或最新地址
                var nextDefault = await _dbContext.Addresses
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync(a => a.UserId == userId);
                
                if (nextDefault != null && !nextDefault.IsDefault)
                {
                    // 将找到的地址设为默认地址
                    nextDefault.IsDefault = true;
                    await _dbContext.SaveChangesAsync();
                }
                
                await SyncDefaultAddressToUser(userId);
            }

            _logger.LogInformation("删除用户地址成功，用户ID: {UserId}, 地址ID: {AddressId}", userId, addressId);
            return true;
        }
        
        /// <summary>
        /// 清除用户相关缓存
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="username">用户名</param>
        private async Task ClearUserCache(int userId, string? username)
        {
            try
            {
                var cacheKeys = new List<string>
                {
                    $"{USER_BY_ID_CACHE_KEY_PREFIX}{userId}",
                    "all_users" // 同时清除用户列表缓存
                };
                
                if (!string.IsNullOrEmpty(username))
                {
                    cacheKeys.Add($"user_username_{username}");
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
        
        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="changePasswordDto">密码修改信息</param>
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("用户ID必须大于0");
            }

            if (changePasswordDto == null)
            {
                throw new ArgumentNullException(nameof(changePasswordDto), "密码信息不能为空");
            }

            _logger.LogInformation("开始修改用户密码，用户ID: {UserId}", userId);

            // 获取用户
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new Exception("用户不存在");
            }

            // 验证当前密码
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password))
            {
                throw new ArgumentException("当前密码不正确");
            }

            // 验证新密码是否符合要求
            if (string.IsNullOrEmpty(changePasswordDto.NewPassword) || changePasswordDto.NewPassword.Length < 6)
            {
                throw new ArgumentException("新密码长度不能少于6位");
            }

            // 哈希新密码并更新
            user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            // 清除用户缓存
            await ClearUserCache(userId, user.Username);

            _logger.LogInformation("用户密码修改成功，用户ID: {UserId}", userId);
            return true;
        }

        private async Task ClearUserAddressesCache(int userId)
        {
            try
            {
                var cacheKey = $"{USER_ADDRESSES_CACHE_KEY_PREFIX}{userId}";
                await _cacheService.RemoveAsync(cacheKey);
                _logger.LogInformation("清除用户地址缓存成功，用户ID: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除用户地址缓存失败，用户ID: {UserId}", userId);
            }
        }

        private async Task<AddressDto?> BuildDtoFromUserAddress(int userId)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return null;
                }
                var addrStr = user.Address ?? string.Empty;
                if (string.IsNullOrWhiteSpace(addrStr))
                {
                    return null;
                }
                string full = addrStr;
                string recipient = user.FullName ?? string.Empty;
                string phone = user.Phone ?? string.Empty;
                var parts = addrStr.Split('|');
                if (parts.Length >= 1) full = parts[0].Trim();
                if (parts.Length >= 2) recipient = string.IsNullOrWhiteSpace(parts[1]) ? recipient : parts[1].Trim();
                if (parts.Length >= 3) phone = string.IsNullOrWhiteSpace(parts[2]) ? phone : parts[2].Trim();
                return new AddressDto
                {
                    Id = 0,
                    RecipientName = recipient,
                    PhoneNumber = phone,
                    Province = "",
                    City = "",
                    District = "",
                    DetailAddress = full,
                    PostalCode = "",
                    IsDefault = true
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
