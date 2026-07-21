using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Exceptions;
using HuanyuFlowerShop.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HuanyuFlowerShop.Tests;

public class ChatServiceTests
{
    [Fact]
    public async Task UserMessage_IsPersistedIdempotently_AndIncrementsAdminUnread()
    {
        await using var db = CreateDb();
        SeedUsers(db);
        var service = new ChatService(db);
        var conversation = await service.GetOrCreateSupportConversationAsync(2);
        var request = new SendMessageRequest
        {
            ConversationId = conversation.Id,
            Content = "  请问今天可以送达吗？  ",
            ClientMessageId = "client-message-1"
        };

        var first = await service.SendMessageAsync(2, false, request);
        var duplicate = await service.SendMessageAsync(2, false, request);

        Assert.Equal(first.Id, duplicate.Id);
        Assert.Equal("请问今天可以送达吗？", first.Content);
        Assert.Single(db.SupportMessages);
        Assert.Equal(1, await service.GetUnreadCountAsync(1, true));
    }

    [Fact]
    public async Task AdminCanAssignReplyAndUserCanMarkConversationRead()
    {
        await using var db = CreateDb();
        SeedUsers(db);
        var service = new ChatService(db);
        var conversation = await service.GetOrCreateSupportConversationAsync(2);

        var assigned = await service.AssignConversationAsync(conversation.Id, 1);
        var reply = await service.SendMessageAsync(1, true, new SendMessageRequest
        {
            ConversationId = conversation.Id,
            Content = "可以，预计两小时内送达。",
            ClientMessageId = "admin-reply-1"
        });

        Assert.Equal("active", assigned?.Status);
        Assert.Equal(2, reply.ReceiverId);
        Assert.Equal(1, await service.GetUnreadCountAsync(2, false));
        Assert.True(await service.MarkConversationReadAsync(conversation.Id, 2, false));
        Assert.Equal(0, await service.GetUnreadCountAsync(2, false));
    }

    [Fact]
    public async Task UserCannotReadAnotherUsersConversation()
    {
        await using var db = CreateDb();
        SeedUsers(db);
        var service = new ChatService(db);
        var conversation = await service.GetOrCreateSupportConversationAsync(2);

        Assert.False(await service.CanAccessConversationAsync(conversation.Id, 3, false));
        Assert.Empty(await service.GetMessagesAsync(conversation.Id, 3, false, 1, 50));
    }

    [Fact]
    public async Task ClosedConversationRejectsNewMessages()
    {
        await using var db = CreateDb();
        SeedUsers(db);
        var service = new ChatService(db);
        var conversation = await service.GetOrCreateSupportConversationAsync(2);
        await service.CloseConversationAsync(conversation.Id, 1);

        await Assert.ThrowsAsync<BusinessException>(() => service.SendMessageAsync(2, false, new SendMessageRequest
        {
            ConversationId = conversation.Id,
            Content = "还有一个问题"
        }));
    }

    [Fact]
    public async Task AssignedConversationCannotBeTakenOverByAnotherAdmin()
    {
        await using var db = CreateDb();
        SeedUsers(db);
        db.Users.Add(new User { Id = 4, Username = "admin2", Email = "admin2@example.com", Password = "hash", Role = "admin", Status = "active" });
        await db.SaveChangesAsync();
        var service = new ChatService(db);
        var conversation = await service.GetOrCreateSupportConversationAsync(2);

        Assert.NotNull(await service.AssignConversationAsync(conversation.Id, 1));
        Assert.Null(await service.AssignConversationAsync(conversation.Id, 4));
        Assert.Equal(1, (await service.GetConversationAsync(conversation.Id, 1, true))?.AdminId);
    }

    [Fact]
    public async Task AttachmentMessagePersistsMetadata_IsIdempotent_AndRequiresConversationAccess()
    {
        await using var db = CreateDb();
        SeedUsers(db);
        var service = new ChatService(db);
        var conversation = await service.GetOrCreateSupportConversationAsync(2);
        var request = new UploadChatAttachmentRequest
        {
            ConversationId = conversation.Id,
            File = new FormFile(Stream.Null, 0, 0, "file", "bouquet.jpg"),
            Caption = "收到的花有一点压痕",
            ClientMessageId = "attachment-1"
        };
        var attachment = new StoredChatAttachment(
            "bouquet.jpg", "2026/07/random.jpg", "image/jpeg", 2048, "image");

        var first = await service.SendAttachmentMessageAsync(2, false, request, attachment);
        var duplicate = await service.SendAttachmentMessageAsync(2, false, request, attachment);

        Assert.True(first.Created);
        Assert.False(duplicate.Created);
        Assert.Equal(first.Message.Id, duplicate.Message.Id);
        Assert.Equal("image", first.Message.MessageType);
        Assert.Equal("bouquet.jpg", first.Message.AttachmentName);
        Assert.True(first.Message.AttachmentAvailable);
        Assert.Single(db.SupportMessages);
        Assert.NotNull(await service.GetAttachmentAsync(first.Message.Id, 2, false));
        Assert.NotNull(await service.GetAttachmentAsync(first.Message.Id, 1, true));
        Assert.Null(await service.GetAttachmentAsync(first.Message.Id, 3, false));
        Assert.Equal(1, await service.GetUnreadCountAsync(1, true));
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedUsers(ApplicationDbContext db)
    {
        db.Users.AddRange(
            new User { Id = 1, Username = "admin", Email = "admin@example.com", Password = "hash", Role = "admin", Status = "active" },
            new User { Id = 2, Username = "customer", Email = "customer@example.com", Password = "hash", Role = "user", Status = "active" },
            new User { Id = 3, Username = "other", Email = "other@example.com", Password = "hash", Role = "user", Status = "active" });
        db.SaveChanges();
    }
}
