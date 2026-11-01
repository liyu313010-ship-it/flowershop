using HuanyuFlowerShop.Entities;
using Microsoft.AspNetCore.Http;

namespace HuanyuFlowerShop.Interfaces
{
    public interface IVideoService
    {
        Task<Video> UploadAsync(IFormFile file, string title, string slot);
        Task<Video?> GetHomeVideoAsync();
        Task<Video?> GetBySlotAsync(string slot);
        Task<Video?> GetByIdAsync(int id);
    }
}
