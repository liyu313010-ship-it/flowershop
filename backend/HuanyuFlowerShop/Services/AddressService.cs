using Microsoft.EntityFrameworkCore;
using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.DTOs;
using Microsoft.Extensions.Logging;

namespace HuanyuFlowerShop.Services
{
    public class AddressService : IAddressService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AddressService> _logger;

        public AddressService(ApplicationDbContext context, ILogger<AddressService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AddressDto>> GetUserAddressesAsync(int userId)
        {
            _logger.LogInformation("获取用户地址列表，用户ID: {UserId}", userId);
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
            return addresses.Any() ? addresses.Select(MapToDto) : Enumerable.Empty<AddressDto>();
        }

        public async Task<AddressDto> GetAddressByIdAsync(int addressId, int userId)
        {
            _logger.LogInformation("获取地址详情，地址ID: {AddressId}, 用户ID: {UserId}", addressId, userId);
            
            var address = await _context.Addresses
                .Where(a => a.Id == addressId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
            {
                _logger.LogWarning("地址不存在，地址ID: {AddressId}, 用户ID: {UserId}", addressId, userId);
                throw new KeyNotFoundException($"地址不存在，ID: {addressId}");
            }

            return MapToDto(address);
        }

        public async Task<AddressDto?> GetDefaultAddressAsync(int userId)
        {
            _logger.LogInformation("获取用户默认地址，用户ID: {UserId}", userId);
            try
            {
                var address = await _context.Addresses
                    .Where(a => a.UserId == userId && a.IsDefault)
                    .FirstOrDefaultAsync();

                if (address != null)
                {
                    return MapToDto(address);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "查询默认地址失败，尝试回退到用户表 Address，用户ID: {UserId}", userId);
            }

            return await BuildDtoFromUserAddress(userId);
        }

        public async Task<AddressDto> CreateAddressAsync(CreateAddressDto createAddressDto, int userId)
        {
            _logger.LogInformation("创建新地址，用户ID: {UserId}", userId);
            
            // 如果设置为默认地址，先将用户其他地址设为非默认
            if (createAddressDto.IsDefault)
            {
                await SetOtherAddressesToNonDefaultAsync(userId);
            }
            else
            {
                // 如果这是用户的第一个地址，自动设为默认
                var hasExistingAddress = await _context.Addresses.AnyAsync(a => a.UserId == userId);
                if (!hasExistingAddress)
                {
                    createAddressDto.IsDefault = true;
                }
            }

            var address = new Address
            {
                UserId = userId,
                RecipientName = createAddressDto.RecipientName ?? string.Empty,
                PhoneNumber = createAddressDto.PhoneNumber ?? string.Empty,
                Province = createAddressDto.Province ?? string.Empty,
                City = createAddressDto.City ?? string.Empty,
                District = createAddressDto.District ?? string.Empty,
                DetailAddress = createAddressDto.DetailAddress ?? string.Empty,
                PostalCode = createAddressDto.PostalCode,
                IsDefault = createAddressDto.IsDefault,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();
            _logger.LogInformation("地址创建成功，地址ID: {AddressId}, 用户ID: {UserId}", address.Id, userId);
            if (address.IsDefault)
            {
                await SyncDefaultAddressToUser(userId);
            }
            return MapToDto(address);
        }

        public async Task<AddressDto> UpdateAddressAsync(int addressId, UpdateAddressDto updateAddressDto, int userId)
        {
            _logger.LogInformation("更新地址，地址ID: {AddressId}, 用户ID: {UserId}", addressId, userId);
            
            var address = await _context.Addresses
                .Where(a => a.Id == addressId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
            {
                _logger.LogWarning("地址不存在，地址ID: {AddressId}, 用户ID: {UserId}", addressId, userId);
                throw new KeyNotFoundException($"地址不存在，ID: {addressId}");
            }

            // 如果设置为默认地址，先将用户其他地址设为非默认
            if (updateAddressDto.IsDefault && !address.IsDefault)
            {
                await SetOtherAddressesToNonDefaultAsync(userId);
            }

            // 更新地址信息
            address.RecipientName = updateAddressDto.RecipientName ?? string.Empty;
            address.PhoneNumber = updateAddressDto.PhoneNumber ?? string.Empty;
            address.Province = updateAddressDto.Province ?? string.Empty;
            address.City = updateAddressDto.City ?? string.Empty;
            address.District = updateAddressDto.District ?? string.Empty;
            address.DetailAddress = updateAddressDto.DetailAddress ?? string.Empty;
            address.PostalCode = updateAddressDto.PostalCode; // 保持可空
            address.IsDefault = updateAddressDto.IsDefault;
            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("地址更新成功，地址ID: {AddressId}, 用户ID: {UserId}", addressId, userId);
            if (address.IsDefault)
            {
                await SyncDefaultAddressToUser(userId);
            }
            return MapToDto(address);
        }

        public async Task DeleteAddressAsync(int addressId, int userId)
        {
            _logger.LogInformation("删除地址，地址ID: {AddressId}, 用户ID: {UserId}", addressId, userId);
            
            var address = await _context.Addresses
                .Where(a => a.Id == addressId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
            {
                _logger.LogWarning("地址不存在，地址ID: {AddressId}, 用户ID: {UserId}", addressId, userId);
                throw new KeyNotFoundException($"地址不存在，ID: {addressId}");
            }

            // 检查是否有相关订单 - 暂时注释掉这部分检查，因为Order实体没有DeliveryAddressId属性
            // var hasOrders = await _context.Orders
            //     .Where(o => o.DeliveryAddressId == addressId)
            //     .AnyAsync();
            var hasOrders = false; // 临时设置为false，以便能够删除地址

            if (hasOrders)
            {
                _logger.LogWarning("该地址已有关联订单，无法删除，地址ID: {AddressId}", addressId);
                throw new InvalidOperationException("该地址已有关联订单，无法删除");
            }

            bool wasDefault = address.IsDefault;
            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            if (wasDefault)
            {
                var nextDefault = await _context.Addresses
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync();

                if (nextDefault != null && !nextDefault.IsDefault)
                {
                    nextDefault.IsDefault = true;
                    nextDefault.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                await SyncDefaultAddressToUser(userId);
            }

            _logger.LogInformation("地址删除成功，地址ID: {AddressId}, 用户ID: {UserId}", addressId, userId);

            // 若用户已无任何地址，清空用户表中的 Address 文本，避免回退残留
            var remain = await _context.Addresses.AnyAsync(a => a.UserId == userId);
            if (!remain)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    user.Address = string.Empty;
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<AddressDto> SetDefaultAddressAsync(int addressId, int userId)
        {
            _logger.LogInformation("设置默认地址，地址ID: {AddressId}, 用户ID: {UserId}", addressId, userId);
            
            var address = await _context.Addresses
                .Where(a => a.Id == addressId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
            {
                _logger.LogWarning("地址不存在，地址ID: {AddressId}, 用户ID: {UserId}", addressId, userId);
                throw new KeyNotFoundException($"地址不存在，ID: {addressId}");
            }

            // 先将用户其他地址设为非默认
            await SetOtherAddressesToNonDefaultAsync(userId);

            // 设置当前地址为默认
            address.IsDefault = true;
            address.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("默认地址设置成功，地址ID: {AddressId}, 用户ID: {UserId}", addressId, userId);
            await SyncDefaultAddressToUser(userId);
            return MapToDto(address);
        }

        // 辅助方法：将用户其他地址设为非默认
        private async Task SetOtherAddressesToNonDefaultAsync(int userId)
        {
            var otherDefaultAddresses = await _context.Addresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();

            foreach (var addr in otherDefaultAddresses)
            {
                addr.IsDefault = false;
                addr.UpdatedAt = DateTime.UtcNow;
            }
        }

        // 辅助方法：映射实体到DTO
        private AddressDto MapToDto(Address address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address), "地址对象不能为空");
            }
            
            return new AddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                RecipientName = address.RecipientName ?? string.Empty,
                PhoneNumber = address.PhoneNumber ?? string.Empty,
                Province = address.Province ?? string.Empty,
                City = address.City ?? string.Empty,
                District = address.District ?? string.Empty,
                DetailAddress = address.DetailAddress ?? string.Empty,
                PostalCode = address.PostalCode ?? string.Empty,
                IsDefault = address.IsDefault,
                CreatedAt = address.CreatedAt,
                UpdatedAt = address.UpdatedAt
            };
        }

        private async Task<AddressDto?> BuildDtoFromUserAddress(int userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    _logger.LogWarning("用户不存在，无法从用户表构建地址，用户ID: {UserId}", userId);
                    return null;
                }

                var addrStr = user.Address ?? string.Empty;
                if (string.IsNullOrWhiteSpace(addrStr))
                {
                    _logger.LogInformation("用户 Address 字段为空，用户ID: {UserId}", userId);
                    return null;
                }

                string full = addrStr;
                string recipient = user.FullName ?? string.Empty;
                string phone = user.Phone ?? string.Empty;

                // 支持按自定义分隔符解析：full|recipient|phone
                var parts = addrStr.Split('|');
                if (parts.Length >= 1) full = parts[0].Trim();
                if (parts.Length >= 2) recipient = string.IsNullOrWhiteSpace(parts[1]) ? recipient : parts[1].Trim();
                if (parts.Length >= 3) phone = string.IsNullOrWhiteSpace(parts[2]) ? phone : parts[2].Trim();

                var dto = new AddressDto
                {
                    Id = 0,
                    UserId = userId,
                    RecipientName = recipient,
                    PhoneNumber = phone,
                    Province = string.Empty,
                    City = string.Empty,
                    District = string.Empty,
                    DetailAddress = full,
                    PostalCode = string.Empty,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("已从用户表 Address 字段构建回退地址，用户ID: {UserId}", userId);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从用户表构建地址失败，用户ID: {UserId}", userId);
                return null;
            }
        }

        private string GenerateFullAddress(Address address)
        {
            return $"{address.Province ?? ""}{address.City ?? ""}{address.District ?? ""}{address.DetailAddress ?? ""} | {address.RecipientName ?? ""} | {address.PhoneNumber ?? ""}";
        }

        private async Task SyncDefaultAddressToUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return;
            }

            var defaultAddress = await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);
            if (defaultAddress != null)
            {
                user.Address = GenerateFullAddress(defaultAddress);
                await _context.SaveChangesAsync();
            }
        }
    }
}
