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
        var message = await context.Messages.AsTracking().FirstOrDefaultAsync(m => m.MessageId == messageId);
        if (message == null)
        {
            throw new Exception("Message not found.");
        }

        message.IsApproved = true;
        message.Timestamp = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    // Get chat messages for a project
    public async Task<List<Message>> GetMessagesAsync(int projectId, bool isClient)
    {
        using var context = _contextFactory.CreateDbContext();
        var chat = await context.Chats
            .Include(c => c.Messages)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(c => c.ProjectId == projectId);

        if (chat == null)
        {
            chat = new Chat
            {
                ProjectId = projectId,
                Messages = new List<Message>() // Initialize an empty list of messages
            };

            context.Chats.Add(chat);
            await context.SaveChangesAsync();
        }

        // Return messages based on approval status and whether the user is a client
        return chat.Messages
            // Only show approved messages for clients and show non deleted messages for rest.
            .Where(message => isClient ? message.IsApproved && !message.IsDeleted 
                : !message.IsDeleted) 
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
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var readMessageIds = await context.ChatReadStatus
                .Where(crs => crs.UserId == userId)
                .Select(crs => crs.MessageId)
                .ToListAsync();

            var unreadMessageCount = await context.Messages
                .Where(message =>
                    message.Chat.ProjectId == projectId &&
                    !readMessageIds.Contains(message.MessageId))
                .CountAsync();

            return unreadMessageCount;
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error getting unread message count for project {projectId} and user {userId}",
                projectId, userId);

            // Optionally rethrow or return a default value
            throw;
        }
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

    public async Task EditMessageAsync(Message message,string content,bool isEditorView)
    {
        using var context = _contextFactory.CreateDbContext();
        var newMessage = await context.Messages.AsTracking().FirstOrDefaultAsync(m => m.MessageId == message.MessageId);
        if (message == null)
        {
            throw new Exception("Message not found.");
        }
        newMessage.Content = content;
        newMessage.IsEdited = true;
        if(isEditorView)
        {
            newMessage.IsApproved = false;
        }

        await context.SaveChangesAsync();
    }
}
