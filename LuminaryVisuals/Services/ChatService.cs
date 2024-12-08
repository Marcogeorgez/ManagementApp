using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using Microsoft.EntityFrameworkCore;

public class ChatService
{
    private readonly ApplicationDbContext context;
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;


    public ChatService(ApplicationDbContext context, IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        this.context = context;
        _contextFactory = contextFactory;
    }

    // Initialize chat for a project
    public async Task InitializeChatAsync(int projectId, string clientId)
    {
        using var context = _contextFactory.CreateDbContext();
        // Check if the chat already exists for the project
        if (!context.Chats.Any(c => c.ProjectId == projectId))
        {
            var chat = new Chat
            {
                ProjectId = projectId,
                Messages = new List<Message>(), // Empty collection of messages initially
            };

            context.Chats.Add(chat);
            await context.SaveChangesAsync();
        }
    }

    // Add a new message to the chat
    public async Task AddMessageAsync(int projectId, string userId, string message, bool isEditor)
    {
        using var context = _contextFactory.CreateDbContext();
        var chat = await context.Chats.FirstOrDefaultAsync(c => c.ProjectId == projectId);
        if (chat == null)
        {
            throw new Exception("Chat for the project not found.");
        }

        // Create new message
        var newMessage = new Message
        {
            ChatId = chat.ChatId,    // Link the message to the existing chat
            UserId = userId,
            Content = message,
            Timestamp = DateTime.UtcNow,
            IsApproved = !isEditor,  // Editor messages are not approved by default
            IsDeleted = false
        };

        context.Messages.Add(newMessage);
        await context.SaveChangesAsync();
    }

    // Approve a message
    public async Task ApproveMessageAsync(int messageId)
    {
        using var context = _contextFactory.CreateDbContext();
        var message = await context.Messages.FindAsync(messageId);
        if (message == null)
        {
            throw new Exception("Message not found.");
        }

        message.IsApproved = true;
        await context.SaveChangesAsync();
    }

    // Get chat messages for a project
    public async Task<List<Message>> GetMessagesAsync(int projectId, bool isClient)
    {
        using var context = _contextFactory.CreateDbContext();
        var chat = await context.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.ProjectId == projectId);

        if (chat == null)
        {
            throw new Exception("Chat not found.");
        }

        // Return messages based on approval status and whether the user is a client
        return chat.Messages
            .Where(message => message.IsApproved &&
                              ( isClient ? !message.IsDeleted : true )) // Only show non-deleted messages for clients
            .OrderBy(message => message.Timestamp)
            .ToList();
    }

    // Track message read status
    public async Task MarkMessagesAsReadAsync(int projectId, string userId)
    {
        using var context = _contextFactory.CreateDbContext();
        var unreadMessages = await context.Messages
            .Where(message => message.Chat.ProjectId == projectId &&
                             !context.ChatReadStatus
                                 .Where(crs => crs.UserId == userId)
                                 .Select(crs => crs.MessageId)
                                 .Contains(message.MessageId))
            .ToListAsync();

        var readStatuses = unreadMessages.Select(message => new ChatReadStatus
        {
            MessageId = message.MessageId,
            UserId = userId,
            IsRead = true,
            ReadTimestamp = DateTime.UtcNow
        });

        context.ChatReadStatus.AddRange(readStatuses);
        await context.SaveChangesAsync();
    }

    // Get unread message count
    public async Task<int> GetUnreadMessageCountAsync(int projectId, string userId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Messages
            .Where(message => message.Chat.ProjectId == projectId &&
                             !context.ChatReadStatus
                                 .Where(crs => crs.UserId == userId)
                                 .Select(crs => crs.MessageId)
                                 .Contains(message.MessageId))
            .CountAsync();
    }

    // Unsend message (logical delete)
    public async Task UnsendMessageAsync(int messageId)
    {
        using var context = _contextFactory.CreateDbContext();
        var message = await context.Messages.FindAsync(messageId);
        if (message == null)
        {
            throw new Exception("Message not found.");
        }

        message.IsDeleted = true;  // Mark the message as deleted (logical deletion)
        await context.SaveChangesAsync();
    }
}
