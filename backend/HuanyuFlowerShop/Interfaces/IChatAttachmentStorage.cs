using HuanyuFlowerShop.DTOs;
using Microsoft.AspNetCore.Http;

namespace HuanyuFlowerShop.Interfaces;

public interface IChatAttachmentStorage
{
    long MaxFileSizeBytes { get; }
    Task<StoredChatAttachment> SaveAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(string storageName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageName, CancellationToken cancellationToken = default);
}
