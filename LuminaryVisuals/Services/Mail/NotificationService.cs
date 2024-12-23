using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
namespace LuminaryVisuals.Services.Mail;

public class NotificationService : BackgroundService, INotificationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, List<NotificationQueueItem>> _notificationQueue;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IServiceProvider serviceProvider, ILogger<NotificationService> logger)
    {
        _serviceProvider = serviceProvider;
        _notificationQueue = new ConcurrentDictionary<string, List<NotificationQueueItem>>();
        _logger = logger;
    }
    public async Task QueueChatNotification(Project project, Message message)
    {
        _logger.LogInformation($"Starting to queue chat notification. MessageId: {message.MessageId}, Project: {project.ProjectName}, Sender: {message.UserId}");
        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<UserServices>();

        var projectUsers = await userService.GetAllUsersAssociatedWithProjectAsync(project);

        if (message.IsApproved)
        {
            var usersToNotify = projectUsers.Where(u => u.Id != message.UserId);
            _logger.LogInformation($"Found {usersToNotify.Count()} users to notify for message {message.UserId} which is an approved message");
            foreach (var user in usersToNotify)
            {
                var notificationItem = new NotificationQueueItem
                {
                    UserId = user.Id,
                    Subject = "New Chat Message",
                    Message = $"New message in project {project.ProjectName}: {message}",
                    CreatedAt = DateTime.UtcNow,
                    MessageId = message.MessageId,
                    ProjectId = project.ProjectId
                };
                await AddToQueueWithReadCheck(notificationItem);
            }
        }
        else if(!message.IsApproved)
        {
            var adminUsers = await userService.GetAllAdminsAsync();
            foreach (var user in adminUsers)
            {
                var notificationItem = new NotificationQueueItem
                {
                    UserId = user.Id,
                    Subject = "New Chat Message",
                    Message = $"New message in project {project.ProjectName}: {message}",
                    CreatedAt = DateTime.UtcNow,
                    MessageId = message.MessageId,
                    ProjectId = project.ProjectId
                };
                await AddToQueueWithReadCheck(notificationItem);
            }
        }
    }
    private async Task AddToQueueWithReadCheck(NotificationQueueItem item)
    {
        if (item.MessageId.HasValue)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _logger.LogDebug($"Checking read status for MessageId: {item.MessageId}, UserId: {item.UserId}");

            var isRead = await context.ChatReadStatus
                .AnyAsync(crs => crs.MessageId == item.MessageId.Value &&
                                crs.UserId == item.UserId &&
                                crs.IsRead);
            if (!isRead)
            {
                _logger.LogInformation($"Message {item.MessageId} not read by user {item.UserId}, adding to queue");
                AddToQueue(item);
            }
            else
            {
                _logger.LogInformation($"Message {item.MessageId} already read by user {item.UserId}, skipping notification");
            }
        }
        else
        {
            _logger.LogInformation($"Adding non-chat notification for user {item.UserId}");
            AddToQueue(item);
        }
    }
    public async Task QueueProjectCreationNotification(Project project)
    {
        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<UserServices>();

        var adminUsers = await userService.GetAllAdminsAsync();

        foreach (var admin in adminUsers)
        {
            var notificationItem = new NotificationQueueItem
            {
                UserId = admin.Id,
                Subject = "New Project Created",
                Message = $"New project '{project.ProjectName}' has been created by {project.ClientName}",
                CreatedAt = DateTime.UtcNow
            };

            AddToQueue(notificationItem);
        }
    }
    public async Task QueueStatusChangeNotification(Project project, ProjectStatus oldStatus, ProjectStatus newStatus)
    {
        _logger.LogInformation($"Queueing status change notification for project {project.ProjectName} from {oldStatus} to {newStatus}");

        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<UserServices>();

        // Always notify admins
        var adminUsers = await userService.GetAllAdminsAsync();
        foreach (var admin in adminUsers)
        {
            var notificationItem = new NotificationQueueItem
            {
                UserId = admin.Id,
                Subject = "Project Status Change",
                Message = $"Project '{project.ProjectName}' status changed from {oldStatus} to {newStatus}",
                CreatedAt = DateTime.UtcNow
            };
            AddToQueue(notificationItem);
        }
        if (newStatus == ProjectStatus.Scheduled || newStatus == ProjectStatus.Revision)
        {
            // Notify editors if they exist
            if (project.PrimaryEditorId != null)
            {
                var primaryEditor = await userService.GetUserByIdAsync(project.PrimaryEditorId);
                if (primaryEditor != null)
                {
                    var notificationItem = new NotificationQueueItem
                    {
                        UserId = primaryEditor.Id,
                        Subject = "Project Status Update",
                        Message = $"Project '{project.ProjectName}' status changed to {newStatus}",
                        CreatedAt = DateTime.UtcNow
                    };
                    AddToQueue(notificationItem);
                }
            }

            if (project.SecondaryEditorId != null)
            {
                var secondaryEditor = await userService.GetUserByIdAsync(project.SecondaryEditorId);
                if (secondaryEditor != null)
                {
                    var notificationItem = new NotificationQueueItem
                    {
                        UserId = secondaryEditor.Id,
                        Subject = "Project Status Update",
                        Message = $"Project '{project.ProjectName}' status changed to {newStatus}",
                        CreatedAt = DateTime.UtcNow
                    };
                    AddToQueue(notificationItem);
                }
            }
        }

        // Notify client for specific statuses
        if (newStatus == ProjectStatus.Scheduled ||
            newStatus == ProjectStatus.Delivered ||
            newStatus == ProjectStatus.Finished)
        {
            if (project.ClientId != null)
            {
                var client = await userService.GetUserByIdAsync(project.ClientId);
                if (client != null)
                {
                    var notificationItem = new NotificationQueueItem
                    {
                        UserId = client.Id,
                        Subject = "Project Status Update",
                        Message = $"Your project '{project.ProjectName}' status has been updated to {newStatus}",
                        CreatedAt = DateTime.UtcNow
                    };
                    AddToQueue(notificationItem);
                }
            }
        }
    }
    private void AddToQueue(NotificationQueueItem item)
    {
        _notificationQueue.AddOrUpdate(
            item.UserId,
            new List<NotificationQueueItem> { item },
            (key, existingList) =>
            {
                existingList.Add(item);
                return existingList;
            });
        _logger.LogInformation($"Added notification to queue for user {item.UserId}. Queue size for this user: {_notificationQueue[item.UserId].Count}");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationService background service started");
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting notification processing cycle");
            await ProcessNotificationQueue();
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }
    }

    private async Task ProcessNotificationQueue()
    {
        _logger.LogInformation($"Processing notification queue. Total users in queue: {_notificationQueue.Count}");
        using var scope = _serviceProvider.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var userService = scope.ServiceProvider.GetRequiredService<UserServices>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var userQueue in _notificationQueue.ToList())
        {
            var userId = userQueue.Key;
            var notifications = userQueue.Value;

            _logger.LogInformation($"Processing notifications for user {userId}. Notification count: {notifications.Count}");

            if (notifications.Any())
            {
                var user = await userService.GetUserByIdAsync(userId);
                if (user?.Email == null)
                {
                    _logger.LogWarning($"User {userId} not found or has no email");
                    continue;
                }

                var validNotifications = new List<NotificationQueueItem>();

                // collect all valid notifications
                foreach (var notification in notifications)
                {
                    if (notification.MessageId.HasValue)
                    {
                        var isRead = await context.ChatReadStatus
                            .AnyAsync(crs => crs.MessageId == notification.MessageId.Value &&
                                            crs.UserId == notification.UserId &&
                                            crs.IsRead);
                        if (!isRead)
                        {
                            _logger.LogDebug($"Message {notification.MessageId} still unread, adding to valid notifications");
                            validNotifications.Add(notification);
                        }
                        else
                        {
                            _logger.LogInformation($"Message {notification.MessageId} was read since queueing, skipping notification");
                        }
                    }
                    else
                    {
                        validNotifications.Add(notification);
                    }
                }

                // Then process the valid notifications
                if (validNotifications.Any())
                {
                    var groupedNotifications = validNotifications  // Changed from notifications to validNotifications
                        .GroupBy(n => n.Subject)
                        .Select(g => new
                        {
                            Subject = g.Key,
                            Messages = g.Select(n => n.Message).ToList()
                        });

                    foreach (var group in groupedNotifications)
                    {
                        if (group.Messages.Any())
                        {
                            string body = group.Messages.Count > 1
                                ? $"You have {group.Messages.Count} new notifications:\n\n{string.Join("\n\n", group.Messages)}"
                                : group.Messages.First();

                            _logger.LogInformation($"Sending email to {user.Email} with {group.Messages.Count} notifications");
                            await emailService.SendEmailAsync(user.Email, group.Subject, body);
                        }
                    }
                }

                // Remove the processed notifications after sending
                if (_notificationQueue.TryRemove(userId, out _))
                {
                    _logger.LogInformation($"Successfully removed processed notifications for user {userId}");
                }
            }
        }
    }

}
    public class NotificationQueueItem
    {
        public string UserId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? MessageId { get; set; }
        public int? ProjectId { get; set; }

    }
