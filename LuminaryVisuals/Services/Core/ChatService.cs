using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Services;
using LuminaryVisuals.Services.Events;
using LuminaryVisuals.Services.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

public class ChatService
{
    private readonly ApplicationDbContext context;
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IMessageNotificationService _messageNotificationService;
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;


    public ChatService(ApplicationDbContext context, IDbContextFactory<ApplicationDbContext> contextFactory, IMessageNotificationService messageNotificationService, INotificationService notificationService, UserManager<ApplicationUser> userManager)
    {
        this.context = context;
        _contextFactory = contextFactory;
        _messageNotificationService = messageNotificationService;
        _notificationService = notificationService;
        _userManager = userManager;
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
    public async Task<Message> AddMessageAsync(int projectId, string userId, string message, bool isEditor)
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
        await _messageNotificationService.NotifyNewMessage(projectId);
        var project = await context.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId);
        if (newMessage.IsApproved == true)
        {
            await _notificationService.QueueChatNotification(project!, newMessage);
        }
        return newMessage;
    }

    // Approve a message
    public async Task ApproveMessageAsync(int projectId, int messageId)
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
        await _messageNotificationService.NotifyNewMessage(projectId);
        var project = await context.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId);
        if (message.IsApproved == true)
        {
            await _notificationService.QueueChatNotification(project!, message);
        }

    }

    // Get chat messages for a project


    public async Task<List<Message>> GetMessagesAsync(int projectId, bool isClient, int pageNumber = 1, int pageSize = 50)
    {
        var chat = await GetOrCreateChatAsync(projectId);

        using var context = _contextFactory.CreateDbContext();
        
        // Return messages based on approval status and whether the user is a client
        var messages = await context.Messages
            .Where(m => m.ChatId == chat.ChatId &&
                        !m.IsDeleted &&
                        ( isClient ? m.IsApproved : true ))
            .OrderByDescending(m => m.Timestamp)
            .Include(m => m.User)
            .Skip(( pageNumber - 1 ) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        messages.Reverse();
        return messages;
    }
    // Creates Chat if it is empty 
    private async Task<Chat> GetOrCreateChatAsync(int projectId)
    {
        using var context = _contextFactory.CreateDbContext();
        var chat = await context.Chats.FirstOrDefaultAsync(c => c.ProjectId == projectId);

        if (chat == null)
        {
            chat = new Chat
            {
                ProjectId = projectId,
                Messages = new List<Message>()
            };

            context.Chats.Add(chat);
            await context.SaveChangesAsync();
        }

        return chat;
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
    public async Task<Tuple<int, DateTime?, int>> GetUnreadMessageCountAndLastMessageTimeAsync(int projectId, string userId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();

            var readMessageIds = await context.ChatReadStatus
                .Where(crs => crs.UserId == userId)
                .Select(crs => crs.MessageId)
                .ToListAsync();

            var query = context.Messages
                .Where(message => message.Chat.ProjectId == projectId);

            // Get unread message count
            var unreadMessageCount = await query
                .Where(message => !readMessageIds.Contains(message.MessageId))
                .CountAsync();

            // Get unapproved message count
            var unapprovedMessageCount = await query
                .Where(message => message.IsApproved == false && !message.IsDeleted)
                .CountAsync();

            // Get the last sent message timestamp (the most recent one)
            var lastSentMessageTimestamp = await query
                .OrderByDescending(message => message.Timestamp)
                .Select(message => message.Timestamp)
                .FirstOrDefaultAsync();

            return new Tuple<int, DateTime?, int>(unreadMessageCount, lastSentMessageTimestamp, unapprovedMessageCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting unread message count for project {projectId} and user {userId}: {ex.Message}");
            throw;
        }
    }
    public async Task<int> GetUnreadMessageCount(string userId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var readMessageIds = await context.ChatReadStatus
                .Where(crs => crs.UserId == userId)
                .Join(context.Messages, crs => crs.MessageId, message => message.MessageId, (crs, message) => new { crs, message })
                .Join(context.Chats, combined => combined.message.ChatId, chat => chat.ChatId, (combined, chat) => new { combined.crs, chat })
                .Join(context.Projects, combined => combined.chat.ProjectId, project => project.ProjectId, (combined, project) => new { combined.crs, project })
                .Where(x => !x.project.IsArchived)
                .Select(x => x.crs.MessageId)
                .ToListAsync();

            // Check if user is admin
            if(string.IsNullOrEmpty(userId))
            {
                return 0;
            }
            var isAdmin = await _userManager.IsInRoleAsync(await _userManager.FindByIdAsync(userId), "Admin");

            if (readMessageIds.Count > 0)
            {
                var unreadMessageCount = await context.Messages
                    .Join(context.Chats, message => message.ChatId, chat => chat.ChatId, (message, chat) => new { message, chat })
                    .Join(context.Projects, combined => combined.chat.ProjectId, project => project.ProjectId, (combined, project) => new { combined.message, project })
                    .Where(x =>
                        !readMessageIds.Contains(x.message.MessageId) &&
                        !x.project.IsArchived &&
                        ( x.project.PrimaryEditorId == userId ||
                         x.project.SecondaryEditorId == userId ||
                         x.project.ClientId == userId ||
                         isAdmin ))
                    .CountAsync();

                return unreadMessageCount;
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting unread message count for user {userId}: {ex.Message}");
            throw;
        }
    }

    // Unsend message (logical delete)
    public async Task UnsendMessageAsync(int projectId, int messageId)
    {
        using var context = _contextFactory.CreateDbContext();
        var message = await context.Messages.AsTracking().FirstOrDefaultAsync(m => m.MessageId == messageId);
        if (message == null)
        {
            throw new Exception("Message not found.");
        }

        message.IsDeleted = true;  // Mark the message as deleted (logical deletion)
        await context.SaveChangesAsync();
        await _messageNotificationService.NotifyNewMessage(projectId);

    }

    public async Task EditMessageAsync(Message message,int projectId,string content,bool isEditorView)
    {
        using var context = _contextFactory.CreateDbContext();
        var newMessage = await context.Messages.AsTracking().FirstOrDefaultAsync(m => m.MessageId == message.MessageId);
        if (message == null)
        {
            throw new Exception("Message not found.");
        }
        newMessage!.Content = content;
        newMessage.IsEdited = true;
        if(isEditorView)
        {
            newMessage.IsApproved = false;
        }

        await context.SaveChangesAsync();
        await _messageNotificationService.NotifyNewMessage(projectId);

    }
}
