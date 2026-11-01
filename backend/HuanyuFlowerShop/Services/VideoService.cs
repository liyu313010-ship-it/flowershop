using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HuanyuFlowerShop.Services
{
    public class VideoService : IVideoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public VideoService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<Video> UploadAsync(IFormFile file, string title, string slot)
        {
            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "videos");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var name = Guid.NewGuid().ToString() + ext;
            var path = Path.Combine(uploads, name);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var existing = _context.Videos.FirstOrDefault(x => x.IsActive && x.Slot == slot);
            if (existing != null)
            {
                existing.IsActive = false;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            var v = new Video
            {
                Title = title,
                FilePath = $"/uploads/videos/{name}",
                Slot = string.IsNullOrWhiteSpace(slot) ? "story" : slot,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Videos.Add(v);
            await _context.SaveChangesAsync();
            return v;
        }

        public async Task<Video?> GetHomeVideoAsync()
        {
            return await Task.FromResult(_context.Videos.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x => x.IsActive));
        }

        public async Task<Video?> GetBySlotAsync(string slot)
        {
            slot = string.IsNullOrWhiteSpace(slot) ? "story" : slot;
            return await Task.FromResult(_context.Videos.Where(x => x.Slot == slot && x.IsActive).OrderByDescending(x => x.CreatedAt).FirstOrDefault());
        }

        public async Task<Video?> GetByIdAsync(int id)
        {
            return await _context.Videos.FindAsync(id);
        }
    }
}
