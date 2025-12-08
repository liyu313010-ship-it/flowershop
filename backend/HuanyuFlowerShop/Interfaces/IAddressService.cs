using HuanyuFlowerShop.DTOs;

namespace HuanyuFlowerShop.Interfaces
{
    public interface IAddressService
    {
        /// <summary>
        /// 获取用户的所有地址
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>地址列表</returns>
        Task<IEnumerable<AddressDto>> GetUserAddressesAsync(int userId);
        
        /// <summary>
        /// 根据ID获取地址
        /// </summary>
        /// <param name="addressId">地址ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns>地址信息</returns>
        Task<AddressDto> GetAddressByIdAsync(int addressId, int userId);
        
        /// <summary>
        /// 获取用户的默认地址
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>默认地址信息</returns>
        Task<AddressDto?> GetDefaultAddressAsync(int userId);
        
        /// <summary>
        /// 创建新地址
        /// </summary>
        /// <param name="createAddressDto">创建地址的请求数据</param>
        /// <param name="userId">用户ID</param>
        /// <returns>创建的地址信息</returns>
        Task<AddressDto> CreateAddressAsync(CreateAddressDto createAddressDto, int userId);
        
        /// <summary>
        /// 更新地址信息
        /// </summary>
        /// <param name="addressId">地址ID</param>
        /// <param name="updateAddressDto">更新地址的请求数据</param>
        /// <param name="userId">用户ID</param>
        /// <returns>更新后的地址信息</returns>
        Task<AddressDto> UpdateAddressAsync(int addressId, UpdateAddressDto updateAddressDto, int userId);
        
        /// <summary>
        /// 删除地址
        /// </summary>
        /// <param name="addressId">地址ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        Task DeleteAddressAsync(int addressId, int userId);
        
        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <param name="addressId">地址ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns>更新后的默认地址信息</returns>
        Task<AddressDto> SetDefaultAddressAsync(int addressId, int userId);
    }
}