using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HuanyuFlowerShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressesController(
        IAddressService addressService,
        ILogger<AddressesController> logger) : ControllerBase
    {
        private readonly IAddressService _addressService = addressService;
        private readonly ILogger<AddressesController> _logger = logger;

        // 获取当前用户ID（转换为int类型）
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("用户未认证或用户ID格式错误");
            }
            return userId;
        }

        // 获取当前用户所有地址
        [HttpGet]
        public async Task<IActionResult> GetAddresses()
        {
            try
            {
                var userId = GetCurrentUserId();
                var addresses = await _addressService.GetUserAddressesAsync(userId);
                return Ok(addresses);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取地址列表失败，返回空列表");
                // 发生异常时返回空列表，避免页面不可用
                return Ok(Array.Empty<AddressDto>());
            }
        }

        // 获取指定地址
        [HttpGet("{addressId}")]
        public async Task<IActionResult> GetAddress(int addressId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var address = await _addressService.GetAddressByIdAsync(addressId, userId);
                return Ok(address);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取地址详情失败", error = ex.Message });
            }
        }

        // 获取默认地址
        [HttpGet("default")]
        public async Task<IActionResult> GetDefaultAddress()
        {
            try
            {
                var userId = GetCurrentUserId();
                var address = await _addressService.GetDefaultAddressAsync(userId);
                if (address == null)
                {
                    return NotFound(new { message = "未设置默认地址" });
                }
                return Ok(address);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取默认地址失败", error = ex.Message });
            }
        }

        // 创建新地址
        [HttpPost]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressDto createAddressDto)
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "参数错误";
                return BadRequest(new { message = firstError, errors = ModelState });
            }

            try
            {
                var userId = GetCurrentUserId();
                var newAddress = await _addressService.CreateAddressAsync(createAddressDto, userId);
                return CreatedAtAction(nameof(GetAddress), new { addressId = newAddress.Id }, newAddress);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "创建地址失败", error = ex.Message });
            }
        }

        // 更新地址
        [HttpPut("{addressId}")]
        public async Task<IActionResult> UpdateAddress(int addressId, [FromBody] UpdateAddressDto updateAddressDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetCurrentUserId();
                var updatedAddress = await _addressService.UpdateAddressAsync(addressId, updateAddressDto, userId);
                return Ok(updatedAddress);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新地址失败", error = ex.Message });
            }
        }

        // 删除地址
        [HttpDelete("{addressId}")]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _addressService.DeleteAddressAsync(addressId, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "删除地址失败", error = ex.Message });
            }
        }

        // 设置默认地址
        [HttpPatch("{addressId}/default")]
        public async Task<IActionResult> SetDefaultAddress(int addressId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var updatedAddress = await _addressService.SetDefaultAddressAsync(addressId, userId);
                return Ok(updatedAddress);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "设置默认地址失败", error = ex.Message });
            }
        }
    }
}
