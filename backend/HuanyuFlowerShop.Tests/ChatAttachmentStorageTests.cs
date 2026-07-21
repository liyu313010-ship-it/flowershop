using HuanyuFlowerShop.Exceptions;
using HuanyuFlowerShop.Options;
using HuanyuFlowerShop.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;

namespace HuanyuFlowerShop.Tests;

public class ChatAttachmentStorageTests : IDisposable
{
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), $"flowershop-chat-{Guid.NewGuid():N}");

    [Fact]
    public async Task ValidImageIsStoredOpenedAndDeleted()
    {
        var storage = CreateStorage();
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4 };
        var file = CreateFile("现场照片.png", png, "image/png");

        var saved = await storage.SaveAsync(file);

        Assert.Equal("现场照片.png", saved.OriginalName);
        Assert.Equal("image", saved.MessageType);
        Assert.Equal("image/png", saved.ContentType);
        await using (var opened = await storage.OpenReadAsync(saved.StorageName))
        {
            Assert.NotNull(opened);
            Assert.Equal(png.Length, opened!.Length);
        }

        await storage.DeleteAsync(saved.StorageName);
        Assert.Null(await storage.OpenReadAsync(saved.StorageName));
    }

    [Fact]
    public async Task SpoofedPdfAndPathTraversalAreRejected()
    {
        var storage = CreateStorage();
        var fakePdf = CreateFile("invoice.pdf", "MZ executable"u8.ToArray(), "application/pdf");

        await Assert.ThrowsAsync<BusinessException>(() => storage.SaveAsync(fakePdf));
        await Assert.ThrowsAsync<BusinessException>(() => storage.OpenReadAsync("../secret.txt"));
    }

    [Fact]
    public async Task OversizedFileIsRejectedBeforeWriting()
    {
        var storage = CreateStorage(maxFileSize: 1024);
        var file = CreateFile("note.txt", Enumerable.Repeat((byte)'a', 1025).ToArray(), "text/plain");
        await Assert.ThrowsAsync<BusinessException>(() => storage.SaveAsync(file));
    }

    [Fact]
    public void AttachmentDirectoryCannotEscapeStorageRoot()
    {
        Directory.CreateDirectory(_tempRoot);
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(value => value.ContentRootPath).Returns(_tempRoot);
        var options = Microsoft.Extensions.Options.Options.Create(new StorageOptions
        {
            RootPath = _tempRoot,
            ChatAttachments = new ChatAttachmentOptions { DirectoryName = "../outside" }
        });

        Assert.Throws<System.InvalidOperationException>(() => new ChatAttachmentStorage(options, environment.Object));
    }

    private ChatAttachmentStorage CreateStorage(long maxFileSize = 10 * 1024 * 1024)
    {
        Directory.CreateDirectory(_tempRoot);
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(value => value.ContentRootPath).Returns(_tempRoot);
        var options = Microsoft.Extensions.Options.Options.Create(new StorageOptions
        {
            RootPath = _tempRoot,
            ChatAttachments = new ChatAttachmentOptions
            {
                DirectoryName = "chat-attachments",
                MaxFileSizeBytes = maxFileSize
            }
        });
        return new ChatAttachmentStorage(options, environment.Object);
    }

    private static FormFile CreateFile(string name, byte[] content, string contentType)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", name)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, true);
        GC.SuppressFinalize(this);
    }
}
