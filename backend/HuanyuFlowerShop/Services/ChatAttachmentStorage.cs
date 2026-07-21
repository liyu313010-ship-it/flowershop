using System.Text;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Exceptions;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.Options;
using Microsoft.Extensions.Options;

namespace HuanyuFlowerShop.Services;

public class ChatAttachmentStorage : IChatAttachmentStorage
{
    private static readonly IReadOnlyDictionary<string, (string ContentType, string MessageType)> AllowedFiles =
        new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = ("image/jpeg", "image"),
            [".jpeg"] = ("image/jpeg", "image"),
            [".png"] = ("image/png", "image"),
            [".gif"] = ("image/gif", "image"),
            [".webp"] = ("image/webp", "image"),
            [".pdf"] = ("application/pdf", "file"),
            [".txt"] = ("text/plain; charset=utf-8", "file"),
            [".doc"] = ("application/msword", "file"),
            [".docx"] = ("application/vnd.openxmlformats-officedocument.wordprocessingml.document", "file"),
            [".xls"] = ("application/vnd.ms-excel", "file"),
            [".xlsx"] = ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "file")
        };

    private readonly string _rootPath;
    public long MaxFileSizeBytes { get; }

    public ChatAttachmentStorage(
        IOptions<StorageOptions> options,
        IWebHostEnvironment environment)
    {
        var storage = options.Value;
        var root = string.IsNullOrWhiteSpace(storage.RootPath) ? "uploads" : storage.RootPath.Trim();
        var rootPath = Path.IsPathRooted(root) ? root : Path.Combine(environment.ContentRootPath, root);
        var directoryName = string.IsNullOrWhiteSpace(storage.ChatAttachments.DirectoryName)
            ? "chat-attachments"
            : storage.ChatAttachments.DirectoryName.Trim();
        var fullStorageRoot = Path.GetFullPath(rootPath);
        _rootPath = Path.GetFullPath(Path.Combine(fullStorageRoot, directoryName));
        var storagePrefix = fullStorageRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!_rootPath.StartsWith(storagePrefix, StringComparison.OrdinalIgnoreCase))
            throw new System.InvalidOperationException("聊天附件目录必须位于 Storage:RootPath 内");
        MaxFileSizeBytes = Math.Clamp(storage.ChatAttachments.MaxFileSizeBytes, 1024, 19L * 1024 * 1024);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<StoredChatAttachment> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length <= 0) throw new BusinessException("请选择要发送的文件");
        if (file.Length > MaxFileSizeBytes)
            throw new BusinessException($"文件大小不能超过{MaxFileSizeBytes / 1024 / 1024}MB");

        var originalName = SanitizeOriginalName(file.FileName);
        var extension = Path.GetExtension(originalName).ToLowerInvariant();
        if (!AllowedFiles.TryGetValue(extension, out var type))
            throw new BusinessException("仅支持 JPG、PNG、GIF、WebP、PDF、TXT、Word 和 Excel 文件");
        if (!await HasValidSignatureAsync(file, extension, cancellationToken))
            throw new BusinessException("文件内容与扩展名不匹配，已拒绝上传");

        var now = DateTime.UtcNow;
        var relativeDirectory = Path.Combine(now.ToString("yyyy"), now.ToString("MM"));
        var directory = ResolvePath(relativeDirectory);
        Directory.CreateDirectory(directory);
        var storageName = Path.Combine(relativeDirectory, $"{Guid.NewGuid():N}{extension}").Replace('\\', '/');
        var destination = ResolvePath(storageName);

        await using var target = new FileStream(
            destination,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        await file.CopyToAsync(target, cancellationToken);

        return new StoredChatAttachment(originalName, storageName, type.ContentType, file.Length, type.MessageType);
    }

    public Task<Stream?> OpenReadAsync(string storageName, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(storageName);
        Stream? stream = File.Exists(path)
            ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan)
            : null;
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageName, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(storageName);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string ResolvePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            throw new BusinessException("附件存储路径无效");
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var rootPrefix = _rootPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("附件存储路径无效");
        return fullPath;
    }

    private static string SanitizeOriginalName(string fileName)
    {
        var value = Path.GetFileName((fileName ?? string.Empty).Replace('\\', '/')).Trim();
        if (value.Length == 0) value = "attachment";
        var invalid = Path.GetInvalidFileNameChars();
        value = new string(value.Select(character => invalid.Contains(character) || char.IsControl(character) ? '_' : character).ToArray());
        if (value.Length <= 180) return value;
        var extension = Path.GetExtension(value);
        return value[..Math.Max(1, 180 - extension.Length)] + extension;
    }

    private static async Task<bool> HasValidSignatureAsync(
        IFormFile file,
        string extension,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var header = new byte[Math.Min(512, (int)Math.Min(file.Length, 512))];
        var read = await stream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        var bytes = header.AsSpan(0, read);

        if (extension is ".docx" or ".xlsx")
        {
            if (!IsZipHeader(bytes) || !stream.CanSeek) return false;
            stream.Position = 0;
            try
            {
                using var archive = new System.IO.Compression.ZipArchive(
                    stream,
                    System.IO.Compression.ZipArchiveMode.Read,
                    leaveOpen: true);
                var expectedDirectory = extension == ".docx" ? "word/" : "xl/";
                return archive.Entries.Any(entry => entry.FullName == "[Content_Types].xml")
                    && archive.Entries.Any(entry => entry.FullName.StartsWith(expectedDirectory, StringComparison.OrdinalIgnoreCase));
            }
            catch (InvalidDataException)
            {
                return false;
            }
        }

        return extension switch
        {
            ".jpg" or ".jpeg" => bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
            ".png" => bytes.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".gif" => bytes.StartsWith("GIF87a"u8) || bytes.StartsWith("GIF89a"u8),
            ".webp" => bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes.Slice(8, 4).SequenceEqual("WEBP"u8),
            ".pdf" => bytes.StartsWith("%PDF-"u8),
            ".doc" or ".xls" => bytes.StartsWith(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }),
            ".txt" => IsUtf8Text(bytes),
            _ => false
        };
    }

    private static bool IsZipHeader(ReadOnlySpan<byte> bytes) =>
        bytes.StartsWith(new byte[] { 0x50, 0x4B, 0x03, 0x04 })
        || bytes.StartsWith(new byte[] { 0x50, 0x4B, 0x05, 0x06 })
        || bytes.StartsWith(new byte[] { 0x50, 0x4B, 0x07, 0x08 });

    private static bool IsUtf8Text(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Contains((byte)0)) return false;
        try
        {
            _ = new UTF8Encoding(false, true).GetString(bytes);
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }
}
