namespace HuanyuFlowerShop.Options;

public class StorageOptions
{
    public string RootPath { get; set; } = "uploads";
    public ChatAttachmentOptions ChatAttachments { get; set; } = new();
}

public class ChatAttachmentOptions
{
    public string DirectoryName { get; set; } = "chat-attachments";
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
}
