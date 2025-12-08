using HuanyuFlowerShop.DTOs;

namespace HuanyuFlowerShop.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId);
        Task<ReviewDto?> GetReviewByIdAsync(int reviewId);
        Task<ReviewDto> CreateReviewAsync(int userId, CreateReviewDto createReviewDto);
        Task<ReviewDto?> UpdateReviewAsync(int userId, int reviewId, UpdateReviewDto updateReviewDto);
        Task<bool> DeleteReviewAsync(int userId, int reviewId);
        Task<decimal> GetAverageProductRatingAsync(int productId);
        Task<int> GetProductReviewCountAsync(int productId);
        Task<bool> HasUserReviewedProductAsync(int userId, int productId);
    }
}
