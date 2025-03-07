using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Services;
using LuminaryVisuals.Services.Events;
using LuminaryVisuals.Services.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class ChatService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IMessageNotificationService _messageNotificationService;
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;


    public ChatService(IDbContextFactory<ApplicationDbContext> contextFactory, IMessageNotificationService messageNotificationService, INotificationService notificationService, UserManager<ApplicationUser> userManager)
    {
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

    public async Task AddMessageAsync(string userId, string updatedByUserId, string message)
    {
        try
        {
            await EnsureAdminChat(userId);
            using var context = _contextFactory.CreateDbContext();
            var chat = await context.Chats.FirstOrDefaultAsync(c => c.UserId == userId);
            // Create new message
            var newMessage = new Message
            {
                ChatId = chat.ChatId,    // Link the message to the existing chat
                UserId = updatedByUserId,
                Content = message,
                Timestamp = DateTime.UtcNow,
                IsApproved = true,  // private messages are approved
                IsDeleted = false
            };
            Console.WriteLine(updatedByUserId);
            context.Messages.Add(newMessage);

            if (chat == null)
            {
                throw new Exception("Chat for the project not found.");
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    // Add a new message to the chat
    public async Task<Message> AddMessageAsync(int projectId, string userId, string message, bool isEditor)
    {
        using var context = _contextFactory.CreateDbContext();
        Chat? chat;
        Message newMessage;
        if (projectId < 0)
        {
            chat = await context.Chats.FirstOrDefaultAsync(c => c.ChatId == -projectId);
            // Create new message
            newMessage = new Message
            {
                ChatId = chat.ChatId,    // Link the message to the existing chat
                UserId = userId,
                Content = message,
                Timestamp = DateTime.UtcNow,
                IsApproved = true,  // private messages are approved
                IsDeleted = false
            };

            context.Messages.Add(newMessage);
        }
        else
        {
            chat = await context.Chats.FirstOrDefaultAsync(c => c.ProjectId == projectId);
            // Create new message
            newMessage = new Message
            {
                ChatId = chat.ChatId,    // Link the message to the existing chat
                UserId = userId,
                Content = message,
                Timestamp = DateTime.UtcNow,
                IsApproved = !isEditor,  // Editor messages are not approved by default
                IsDeleted = false
            };

            context.Messages.Add(newMessage);
        }
        if (chat == null)
        {
            throw new Exception("Chat for the project not found.");
        }

        await context.SaveChangesAsync();
        await _messageNotificationService.NotifyNewMessage(projectId);
        if (projectId > 0)
        {
            var project = await context.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId);
            _ = Task.Run(() => _notificationService.QueueChatNotification(project!, newMessage));
        }
        else
        {
            _ = Task.Run(() => _notificationService.QueuePrivateChatNotification(userId,newMessage,chat));

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

    // Get messages for a project chat

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
    // Creates Chat if it is empty only for non-admin chats.
    private async Task<Chat> GetOrCreateChatAsync(int projectId)
    {
        if (projectId > 0)
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
        // If projectId < 0, that's mean it's an admin-chat therefore the id of the projectId is the same as chatId but the non-negative value of it.
        else
        {
            using var context = _contextFactory.CreateDbContext();
            var chat = await context.Chats.FirstOrDefaultAsync(c => c.ChatId == -projectId);
            if (chat == null)
            {
                chat = new Chat
                {
                    ChatId = -projectId,
                    Messages = new List<Message>()
                };
                context.Chats.Add(chat);
                await context.SaveChangesAsync();
            }
            return chat;
        }
    }

    // Track message read status
    public async Task MarkMessagesAsReadAsync(int projectId, string userId)
    {
        using var context = _contextFactory.CreateDbContext();
        List<Message> unreadMessages;
        if (projectId > 0)
        {
             unreadMessages = await context.Messages
                .Where(message => message.Chat.ProjectId == projectId &&
                                 !context.ChatReadStatus
                                     .Where(crs => crs.UserId == userId)
                                     .Select(crs => crs.MessageId)
                                     .Contains(message.MessageId))
                .ToListAsync();
        }
        else
        {
            unreadMessages = await context.Messages
            .Where(message => message.ChatId == -projectId &&
                             !context.ChatReadStatus
                                 .Where(crs => crs.UserId == userId)
                                 .Select(crs => crs.MessageId)
                                 .Contains(message.MessageId))
            .ToListAsync();
        }
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
    /*    public async Task<Tuple<int, DateTime?, int>> GetUnreadMessageCountAndLastMessageTimeAsync(int projectId, string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                var readMessageIds = await context.ChatReadStatus
                    .Where(crs => crs.UserId == userId)
                    .Select(crs => crs.MessageId)
                    .ToListAsync();
                IQueryable<Message> query;
                if (projectId > 0)
                {
                     query = context.Messages
                        .Where(message => message.Chat.ProjectId == projectId);

                }
                else
                {
                    query = context.Messages
                        .Where(message => message.ChatId == -projectId);
                }
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
    */

    public async Task<Dictionary<int, Tuple<int, DateTime?, int>>> GetUnreadMessageDataForProjectsAsync(List<int> projectIds, string userId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            
            // Get read message IDs for this user once
            var readMessageIds = await context.ChatReadStatus
                .Where(crs => crs.UserId == userId)
                .Select(crs => crs.MessageId)
                .ToListAsync();

            // Get all messages for all projects in a single query
            var allProjectMessages = await context.Messages
                .Where(message => message.Chat.ProjectId.HasValue && projectIds.Contains(message.Chat.ProjectId.Value))
                .Select(m => new
                {
                    m.MessageId,
                    m.Chat.ProjectId,
                    m.Timestamp,
                    m.IsApproved,
                    m.IsDeleted
                })
                .ToListAsync();

            // Group by project ID first to avoid repeated filtering
            var messagesByProject = allProjectMessages.GroupBy(m => m.ProjectId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new Dictionary<int, Tuple<int, DateTime?, int>>();

            // Process each project's data
            foreach (var projectId in projectIds)
            {
                if (messagesByProject.TryGetValue(projectId, out var projectMessages))
                {
                    int unreadCount = projectMessages.Count(m => !readMessageIds.Contains(m.MessageId));
                    int unapprovedCount = projectMessages.Count(m => m.IsApproved == false && !m.IsDeleted);
                    DateTime? lastMessageTime = projectMessages.Any()
                        ? projectMessages.Max(m => m.Timestamp)
                        : null;

                    result[projectId] = new Tuple<int, DateTime?, int>(unreadCount, lastMessageTime, unapprovedCount);
                }
                else
                {
                    // Handle case where project has no messages
                    result[projectId] = new Tuple<int, DateTime?, int>(0, null, 0);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting unread message data: {ex.Message}");
            throw;
        }
    }
    public async Task<int> GetUnreadMessageCount(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return 0;
        }

        try
        {
            using var context = _contextFactory.CreateDbContext();
            var isAdmin = await _userManager.IsInRoleAsync(
                await _userManager.FindByIdAsync(userId),
                "Admin"
            );

            // Get all read message IDs for the user
            var readMessageIds = await context.ChatReadStatus
                .Where(crs => crs.UserId == userId)
                .Select(crs => crs.MessageId)
                .ToListAsync();

            // Start with base query for messages
            var query = context.Messages
                .Join(context.Chats,
                    message => message.ChatId,
                    chat => chat.ChatId,
                    (message, chat) => new { message, chat });

            // Split query based on chat type
            var projectMessagesQuery = query
                .Where(x => x.chat.ProjectId != null)
                .Join(context.Projects,
                    combined => combined.chat.ProjectId,
                    project => project.ProjectId,
                    (combined, project) => new { combined.message, project })
                .Where(x =>
                    !x.project.IsArchived &&
                    ( x.project.PrimaryEditorId == userId ||
                     x.project.SecondaryEditorId == userId ||
                     x.project.ClientId == userId ||
                     isAdmin ));

            var adminChatMessagesQuery = query
                .Where(x =>
                    x.chat.IsAdminChat &&
                    ( x.chat.UserId == userId || isAdmin ));

            // Combine both queries
            var unreadMessageCount = await projectMessagesQuery
                .Select(x => x.message.MessageId)
                .Union(
                    adminChatMessagesQuery
                        .Select(x => x.message.MessageId)
                )
                .Where(messageId => !readMessageIds.Contains(messageId))
                .CountAsync();

            return unreadMessageCount;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    // Unsend message (logical delete)
    // Uses projectId to notify the chat participants of the change
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









    /// <summary>
    /// Ensures that the admin chat exists for the user and only 1 such chat exists.
    /// </summary>
    /// <param name="userId">Id of current user that is logged-in</param>
    /// <returns></returns>
    public async Task EnsureAdminChat(string userId)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var isAdmin = await _userManager.IsInRoleAsync(await _userManager.FindByIdAsync(userId), "Admin");

            if (!isAdmin)
            {
                var existingAdminChat = await context.Chats
                    .FirstOrDefaultAsync(c => c.IsAdminChat && c.UserId == userId);

                if (existingAdminChat == null)
                {
                    var newAdminChat = new Chat
                    {
                        IsAdminChat = true,
                        UserId = userId,
                        Messages = new List<Message>()
                    };

                    context.Chats.Add(newAdminChat);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
