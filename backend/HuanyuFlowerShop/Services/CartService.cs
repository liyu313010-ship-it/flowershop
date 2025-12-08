using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.DTOs;

namespace HuanyuFlowerShop.Services
{
    public class CartService : ICartService
    {
        private readonly IRepository<CartItem> _cartRepository;
        private readonly IRepository<Product> _productRepository;

        public CartService(IRepository<CartItem> cartRepository, IRepository<Product> productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<CartItemDto>> GetCartItemsAsync(int userId)
        {
            var cartItems = await _cartRepository.GetAllAsync();
            var userCartItems = cartItems.Where(ci => ci.UserId == userId);

            var result = new List<CartItemDto>();
            
            foreach (var cartItem in userCartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product != null)
                {
                    result.Add(new CartItemDto
                    {
                        Id = cartItem.Id,
                        ProductId = cartItem.ProductId,
                        ProductName = product.Name ?? "",
                        ProductImage = product.ImageUrl ?? "",
                        Price = product.Price,
                        Quantity = cartItem.Quantity,
                        TotalPrice = product.Price * cartItem.Quantity,
                        CreatedAt = cartItem.CreatedAt
                    });
                }
            }

            return result;
        }

        public async Task<CartItemDto> AddToCartAsync(int userId, AddToCartDto addToCartDto)
        {
            // 检查产品是否存在
            var product = await _productRepository.GetByIdAsync(addToCartDto.ProductId);
            if (product == null)
            {
                throw new ArgumentException("产品不存在");
            }

            // 检查库存
            if (product.Stock < addToCartDto.Quantity)
            {
                throw new ArgumentException("库存不足");
            }

            // 检查购物车中是否已有该产品
            var cartItems = await _cartRepository.GetAllAsync();
            var existingCartItem = cartItems.FirstOrDefault(ci => ci.UserId == userId && ci.ProductId == addToCartDto.ProductId);

            if (existingCartItem != null)
            {
                // 更新数量
                var newQuantity = existingCartItem.Quantity + addToCartDto.Quantity;
                if (product.Stock < newQuantity)
                {
                    throw new ArgumentException("库存不足");
                }

                existingCartItem.Quantity = newQuantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow; // 更新时间戳
                await _cartRepository.UpdateAsync(existingCartItem);

                return new CartItemDto
                {
                    Id = existingCartItem.Id,
                    ProductId = existingCartItem.ProductId,
                    ProductName = product.Name ?? "",
                    ProductImage = product.ImageUrl ?? "",
                    Price = product.Price,
                    Quantity = existingCartItem.Quantity,
                    TotalPrice = product.Price * existingCartItem.Quantity,
                    CreatedAt = existingCartItem.CreatedAt
                };
            }
            else
            {
                // 添加新的购物车项
                var cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = addToCartDto.ProductId,
                    Quantity = addToCartDto.Quantity,
                    Price = product.Price, // 设置商品价格
                    CreatedAt = DateTime.UtcNow
                };

                var createdCartItem = await _cartRepository.AddAsync(cartItem);

                return new CartItemDto
                {
                    Id = createdCartItem.Id,
                    ProductId = createdCartItem.ProductId,
                    ProductName = product.Name ?? "",
                    ProductImage = product.ImageUrl ?? "",
                    Price = product.Price,
                    Quantity = createdCartItem.Quantity,
                    TotalPrice = product.Price * createdCartItem.Quantity,
                    CreatedAt = createdCartItem.CreatedAt
                };
            }
        }

        public async Task<CartItemDto?> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemDto updateCartItemDto)
        {
            var cartItem = await _cartRepository.GetByIdAsync(cartItemId);
            if (cartItem == null || cartItem.UserId != userId)
            {
                return null;
            }

            // 检查产品库存
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
            if (product == null || product.Stock < updateCartItemDto.Quantity)
            {
                throw new ArgumentException("库存不足");
            }

            cartItem.Quantity = updateCartItemDto.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow; // 更新时间戳
            var updated = await _cartRepository.UpdateAsync(cartItem);

            if (!updated)
            {
                return null;
            }

            return new CartItemDto
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductName = product.Name ?? "",
                ProductImage = product.ImageUrl ?? "",
                Price = product.Price,
                Quantity = cartItem.Quantity,
                TotalPrice = product.Price * cartItem.Quantity,
                CreatedAt = cartItem.CreatedAt
            };
        }

        public async Task<bool> RemoveFromCartAsync(int userId, int cartItemId)
        {
            var cartItem = await _cartRepository.GetByIdAsync(cartItemId);
            if (cartItem == null || cartItem.UserId != userId)
            {
                return false;
            }

            return await _cartRepository.DeleteAsync(cartItemId);
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var cartItems = await _cartRepository.GetAllAsync();
            var userCartItems = cartItems.Where(ci => ci.UserId == userId);

            foreach (var cartItem in userCartItems)
            {
                await _cartRepository.DeleteAsync(cartItem.Id);
            }

            return true;
        }
    }
}