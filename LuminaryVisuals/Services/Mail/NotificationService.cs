using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Services.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
namespace LuminaryVisuals.Services.Mail;

public class NotificationService : BackgroundService, INotificationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, List<NotificationQueueItem>> _notificationQueue;
    private readonly ILogger<NotificationService> _logger;
    private readonly IConfiguration _configuration;

    public NotificationService(IServiceProvider serviceProvider, ILogger<NotificationService> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _notificationQueue = new ConcurrentDictionary<string, List<NotificationQueueItem>>();
        _logger = logger;
        _configuration = configuration;
    }
    public async Task QueueChatNotification(Project project, Message message)
    {
        try
        {
            _logger.LogInformation($"Starting to queue chat notification. MessageId: {message.MessageId}, Project: {project.ProjectName}, Sender: {message.UserId}");
            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UserServices>();

            var projectUsers = await userService.GetAllUsersAssociatedWithProjectAsync(project);
            var projectName = project.ProjectId > 0 ? $"project {project.ProjectName}" : $"private chat with {project.Client.UserName}";
            if (message.IsApproved)
            {
                var usersToNotify = projectUsers.Where(u => u.Id != message.UserId);
                _logger.LogInformation($"Found {usersToNotify.Count()} users to notify for message {message.UserId} which is an approved message");
                foreach (var user in usersToNotify)
                {
                    var notificationItem = new NotificationQueueItem
                    {
                        UserId = user.Id,
                        Subject = "You have a new message on Synchron ⚡",
                        Message = $"Hello there, you have received a new message on the project {projectName}.\n Please reply when you can!",
                        CreatedAt = DateTime.UtcNow,
                        MessageId = message.MessageId,
                        ProjectId = project.ProjectId
                    };
                    await AddToQueueWithReadCheck(notificationItem);
                }
            }
            else if (!message.IsApproved)
            {
                var adminUsers = await userService.GetAllAdminsAsync();
                foreach (var user in adminUsers)
                {
                    var notificationItem = new NotificationQueueItem
                    {
                        UserId = user.Id,
                        Subject = "You have a new message approval request on Synchron ⚡",
                        Message = $"Hello there, you have received a new message approval request  on the project {projectName} by editor {project.PrimaryEditor}. Review asap!",
                        CreatedAt = DateTime.UtcNow,
                        MessageId = message.MessageId,
                        ProjectId = project.ProjectId
                    };
                    await AddToQueueWithReadCheck(notificationItem);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while queuing chat notification.");
        }
    }
    public async Task QueuePrivateChatNotification(string userId, Message message, Chat chat)
    {
        try
        {
            _logger.LogInformation($"Starting to queue chat notification. MessageId: {message.MessageId}, private chat , Sender: {message.UserId}");
            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UserServices>();

            List<ApplicationUser> projectUsers = await userService.GetAllAdminsAsync();
            var user = await userService.GetUserByIdAsync(chat.UserId!); // owner of the chat ( the one who's not an admin )

            var projectName = userId == user.Id
                ? $"private chat with {user.UserName}"
                : "Management Chat";
            projectUsers.Add(user);
            var usersToNotify = projectUsers.Where(u => u.Id != message.UserId);
            _logger.LogInformation($"Found {usersToNotify.Count()} users to notify for message {message.UserId} which is an approved message");
            foreach (var _user in usersToNotify)
            {
                var notificationItem = new NotificationQueueItem
                {
                    UserId = _user.Id,
                    Subject = "You have a new message on Synchron ⚡",
                    Message = $"Hello there, you have received a new message on the {projectName}.\n Please reply when you can!",
                    CreatedAt = DateTime.UtcNow,
                    MessageId = message.MessageId,
                };
                await AddToQueueWithReadCheck(notificationItem);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while queuing chat notification.");
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
                await AddToQueue(item);
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
                Subject = $"The project {project.ProjectName} has been added by {project.Client.UserName}⚡",
                Message = $@"
                            <p>The project for the client <strong>{project.Client.UserName}</strong> has been added as <strong>{project.FormatStatus}</strong> on 
                            <a href='https://synchron.luminaryvisuals.net/project' target='_blank'>Synchron</a>.</p>",
                CreatedAt = DateTime.UtcNow
            };

            AddToQueue(notificationItem);
        }
    }
    public async Task QueueProjectScheduleUpdated(Project project, string UserId)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UserServices>();

            var adminUsers = await userService.GetAllAdminsAsync();
            var user = await userService.GetUserByIdAsync(UserId);

            foreach (var admin in adminUsers)
            {
                var notificationItem = new NotificationQueueItem
                {
                    UserId = admin.Id,
                    Subject = $"The project {project.ProjectName} Schedule has been modified by {user.UserName}⚡",
                    Message = $@"
                            <p>The project for the client <strong>{project.Client.UserName}</strong> Schedule has been changed to<strong>Start Date:  {project.StartDate?.Date.ToString("MM-dd-yyyy")} and End Date:  {project.EndDate?.Date.ToString("MM-dd-yyyy")} </strong> on 
                            <a href='https://synchron.luminaryvisuals.net/project' target='_blank'>Synchron</a>.</p>",
                    CreatedAt = DateTime.UtcNow
                };

                AddToQueue(notificationItem);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError($"{ex}");
        }
    }
    public async Task NewUserJoinedNoitifcation(ApplicationUser User)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UserServices>();

            var adminUsers = await userService.GetAllAdminsAsync();

            foreach (var admin in adminUsers)
            {
                var notificationItem = new NotificationQueueItem
                {
                    UserId = admin.Id,
                    Subject = $"New User {User.UserName} has requested to join Synchron ⚡",
                    Message = $@"
                            <p>The user <strong>{User.UserName}</strong> with email {User.Email} has joined as a guest on 
                            <a href='https://synchron.luminaryvisuals.net/project' target='_blank'>Synchron</a> please review it and assign a role to them.</p>",
                    CreatedAt = DateTime.UtcNow
                };
                AddToQueue(notificationItem);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }
    public async Task ClientPreferencesUpdated(ApplicationUser clientUser,string UserId, Dictionary<string, string> newClientPreferences)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UserServices>();
            var projectService = scope.ServiceProvider.GetRequiredService<ProjectService>();

            // Fetch admin users and extract IDs directly
            var adminUsersIds = ( await userService.GetAllAdminsAsync() )
                .Select(u => u.Id)
                .ToHashSet(); // HashSet to ensures uniqueness

            // Fetch projects and extract editor IDs for scheduled/working/revision projects
            var editorIds = ( await projectService.GetProjectsForClients(false, clientUser.Id) )
                .Where(p => p.Status == ProjectStatus.Scheduled
                    || p.Status == ProjectStatus.Working
                    || p.Status == ProjectStatus.Revision)
                .SelectMany(p => new[] { p.PrimaryEditorId, p.SecondaryEditorId })
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet();

            // Combine both sets
            adminUsersIds.UnionWith(editorIds);

            // Convert the result to a list if needed
            List<string> users = adminUsersIds.ToList();

            foreach (var user in users)
            {
                if (UserId != user)
                {
                    var notificationItem = new NotificationQueueItem
                    {
                        UserId = user,
                        Subject = $"Client {clientUser.UserName} Preferences Updated at Synchron ⚡",
                        Message = $@"
                            <p>The user <strong>{clientUser.UserName}</strong> with email {clientUser.Email} has updated their preferences.
                            <br> The new preferences are: <br> {string.Join("<br>", newClientPreferences.Select(kvp => $"<strong>{kvp.Key}</strong>: {kvp.Value}"))}


                            <a href='https://synchron.luminaryvisuals.net/project' target='_blank'>Synchron</a> please review to stay up to date.</p>",
                        CreatedAt = DateTime.UtcNow
                    };
                    AddToQueue(notificationItem);
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }
    public async Task QueueStatusChangeNotification(Project project, ProjectStatus oldStatus, ProjectStatus newStatus, string updatedByUserId)
    {
        _logger.LogInformation($"Queueing status change notification for project {project.ProjectName} from {oldStatus} to {newStatus}");
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UserServices>();
            var readyToReviewText = newStatus == ProjectStatus.Ready_To_Review ? $"Make sure to check it before {project.FormattedDueDate}" : "";
            // Always notify admins
            var adminUsers = await userService.GetAllAdminsAsync();
            foreach (var admin in adminUsers.Where(u => u.Id != updatedByUserId))
            {
                var secondPrimaryEditorNameMessage = project.SecondaryEditorName != null ? $" and <strong>{project.SecondaryEditorName}</strong>" : "";
                var messageForPrivateNotes = ( newStatus == ProjectStatus.Delivered && project.NotesForProject != null && project.NotesForProject?.Length > 15 ? $"<br><strong> This Project has private notes:</strong> {project.NotesForProject}" : "" );
                var notificationItem = new NotificationQueueItem
                {
                    UserId = admin.Id,
                    Subject = $"The Project {project.ProjectName} has changed status {( newStatus == ProjectStatus.Delivered && project.NotesForProject != null && project.NotesForProject?.Length > 15 ? "to delivered and has private notes" : "" )}",
                    Message = $@"
                            <p>The project for the client <strong>{project.Client.UserName}</strong> edited by <strong>{project.PrimaryEditorName}</strong> <strong>{secondPrimaryEditorNameMessage}</strong> 
                             status changed from <strong>{oldStatus.ToString().Replace('_', ' ')}</strong> to <strong>{newStatus.ToString().Replace('_', ' ')}</strong> on 
                            <a href='https://synchron.luminaryvisuals.net/project' target='_blank'>Synchron</a>.
                            {readyToReviewText}{messageForPrivateNotes}</p>",
                    CreatedAt = DateTime.UtcNow
                };
                AddToQueue(notificationItem);
            }
            if (newStatus == ProjectStatus.Revision)
            {
                // Notify editors if they exist
                if (project.PrimaryEditorId != null)
                {
                    var primaryEditor = await userService.GetUserByIdAsync(project.PrimaryEditorId);
                    if (primaryEditor != null && project.PrimaryEditorId != updatedByUserId)
                    {
                        var notificationItem = new NotificationQueueItem
                        {
                            UserId = primaryEditor.Id,
                            Subject = $"Project {project.ProjectName} Status Update",
                            Message = $"Project '{project.ProjectName}' status changed to {newStatus}",
                            CreatedAt = DateTime.UtcNow
                        };
                        AddToQueue(notificationItem);
                    }
                }

                if (project.SecondaryEditorId != null && project.SecondaryEditorId != updatedByUserId)
                {
                    var secondaryEditor = await userService.GetUserByIdAsync(project.SecondaryEditorId);
                    if (secondaryEditor != null)
                    {
                        var notificationItem = new NotificationQueueItem
                        {
                            UserId = secondaryEditor.Id,
                            Subject = $"Project {project.ProjectName} Status Update",
                            Message = $"Project '{project.ProjectName}' status changed to {newStatus}",
                            CreatedAt = DateTime.UtcNow
                        };
                        AddToQueue(notificationItem);
                    }
                }
            }

            // Notify client for specific statuses
            if (newStatus == ProjectStatus.Scheduled || newStatus == ProjectStatus.Finished)
            {
                if (project.ClientId != null && project.ClientId != updatedByUserId)
                {
                    var client = await userService.GetUserByIdAsync(project.ClientId);
                    if (client != null)
                    {
                        var notificationItem = new NotificationQueueItem
                        {
                            UserId = client.Id,
                            Subject = $"Your Project {project.ProjectName} has updates ⚡",
                            Message = $@" <p>The project '<strong>{project.ProjectName}</strong>' has been marked as '<strong>{newStatus}</strong>' on <a href='https://synchron.luminaryvisuals.net/project' target='_blank'>Synchron</a>.</p>",
                            CreatedAt = DateTime.UtcNow
                        };
                        AddToQueue(notificationItem);
                    }
                }
            }
            if (newStatus == ProjectStatus.Delivered)
            {
                if (project.ClientId != null && project.ClientId != updatedByUserId)
                {
                    var client = await userService.GetUserByIdAsync(project.ClientId);
                    if (client != null)
                    {
                        var notificationItem = new NotificationQueueItem
                        {
                            UserId = client.Id,
                            Subject = $"You have a new project delivered by Luminary Visuals ⚡",
                            Message = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Project Delivered</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f9;
            color: #333;
            padding: 20px;
        }}
        *{{text-align: center; }}
        .container {{
            background-color: #ffffff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        }}
        a {{
            color: #007bff;
            text-decoration: none;
        }}
        a:hover {{
            text-decoration: underline;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>The project '<strong>{project.ProjectName}</strong>' has updates⚡</h2>
        <p>Hello there, the project '<strong>{project.ProjectName}</strong>' has been marked as '<strong>{newStatus}</strong>' on 
        <a href='https://synchron.luminaryvisuals.net/project' target='_blank'>Synchron</a>.
        <br/> If you have any specific revisions please add them with timestamps on the <a href='{project.Link}' target='_blank'>Dropbox file</a> </p>
    </div>
</body>
</html>",
                            CreatedAt = DateTime.UtcNow
                        };
                        AddToQueue(notificationItem);
                    }
                }
            }
        }
        catch(Exception ex)
        {
            _logger.LogError($"{ex}");
        }
    }
    private async Task AddToQueue(NotificationQueueItem item)
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
        // Also send as push notification
        await SendPushNotificationForQueueItemAsync(item);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationService background service started");
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting notification processing cycle");
            await ProcessNotificationQueue();
            int delayMinutes = _configuration.GetValue("EMAIL_SEND_DELAY_MINUTES", 30); // Default to 30 minutes if not set
            TimeSpan delayTimeSpan = TimeSpan.FromMinutes(delayMinutes);
            await Task.Delay(delayTimeSpan, stoppingToken);
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
                // Get ALL associated emails
                var associatedEmails = await userService.GetAssociatedEmailsAsync(userId);
                var allEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { user.Email };
                allEmails.UnionWith(associatedEmails);



                var validNotifications = new List<NotificationQueueItem>();

                // collect all valid notifications (this is specifically for checking if a notification is a message
                // and if it has been read by the user then we don't send it)
                foreach (var notification in notifications)
                {
                    if (notification.MessageId.HasValue)
                    {
                        var messageExists = await context.Messages
                            .AnyAsync(m => m.MessageId == notification.MessageId.Value && !m.IsDeleted);
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

                // Then process the remaining notifications
                foreach (var notification in validNotifications)
                {
                    if (!string.IsNullOrEmpty(notification.Message))
                    {
                        foreach (var email in allEmails)
                        {
                            _logger.LogInformation($"Sending email to {email} for notification with subject {notification.Subject}");
                            string body = notification.Message;

                            await emailService.SendEmailAsync(email, notification.Subject, body, true);
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
    private async Task SendPushNotificationForQueueItemAsync(NotificationQueueItem item)
    {
        try
        {
            _logger.LogInformation($"Sending Push notification for user: {item.UserId}");

            // Get the user from the database
            using var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var pushNotification = scope.ServiceProvider.GetRequiredService<PushNotificationService>();
            var user = await userManager.FindByIdAsync(item.UserId);
            if (user == null)
            {
                return; // Skip if no push notification details
            }

            // Send the push notification
            await pushNotification.SendNotification(user, item.Subject, item.Message, item.MessageId);

            _logger.LogInformation($"Push notification sent for user {item.UserId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending push notification to user {item.UserId}");
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
