using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Entities;

namespace HuanyuFlowerShop.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
        Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto);
        string GenerateJwtToken(User user);
        Task<UpdateUserResponseDto> UpdateUserAvatarAsync(int userId, string avatarUrl);
        Task<UpdateUserResponseDto> UpdateUserProfileAsync(int userId, UpdateUserDto updateUserDto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<IEnumerable<AddressDto>> GetAddressesAsync(int userId);
        Task<AddressDto> AddAddressAsync(int userId, AddressDto addressDto);
        Task<AddressDto> UpdateAddressAsync(int userId, int addressId, AddressDto addressDto);
        Task<bool> DeleteAddressAsync(int userId, int addressId);
    }
}