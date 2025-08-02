using ManagementApp.Data;
using ManagementApp.Data.Entities;
using ManagementApp.Services.Core;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

public class UserNotificationService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly UserServices userServices;

    public UserNotificationService(IDbContextFactory<ApplicationDbContext> context, UserServices userServices)
    {
        _contextFactory = context;
        this.userServices = userServices;
    }

    // Creates a new notification
    public async Task CreateNotification(string message, string? targetRole, Severity Severity)
    {
        using var _context = _contextFactory.CreateDbContext();

        var notification = new Notification
        {
            Message = message,
            CreatedAt = DateTime.UtcNow,
            TargetRole = targetRole,
            SeverityColor = Severity
        };
        if (notification.TargetRole == "Everyone")
        {
            notification.TargetRole = null;
        }

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _ = Task.Run(() => CreateUserNotification(notification));
    }

    public async Task CreateUserNotification(Notification notification)
    {
        List<string> users = await userServices.GetAllUsersForRoleAsync(notification.TargetRole);
        if (users.Count == 0)
        {
            return;
        }
        else
        {
            using var _context = _contextFactory.CreateDbContext();
            foreach (var user in users)
            {
                var userNotificationStatus = new UserNotificationStatus
                {
                    UserId = user,
                    NotificationId = notification.Id,
                    Dismissed = false
                };
                _context.UserNotificationStatuses.Add(userNotificationStatus);
            }
            await _context.SaveChangesAsync();
        }
    }
    // Get active notifications for a user
    public async Task<List<UserNotificationStatus>> GetActiveNotificationsForUser(string userId, string userRole)
    {
        using var _context = _contextFactory.CreateDbContext();
        return await _context.UserNotificationStatuses
            .Include(n => n.Notification)
            .Where(uns => uns.UserId == userId && uns.Dismissed == false)
            .ToListAsync();
    }

    // Dismiss a notification for a user
    public async Task DismissNotification(string userId, int notificationId)
    {
        using var _context = _contextFactory.CreateDbContext();

        var status = await _context.UserNotificationStatuses
            .AsTracking()
            .FirstOrDefaultAsync(uns =>
                uns.UserId == userId &&
                uns.NotificationId == notificationId);

        if (status == null)
        {
            status = new UserNotificationStatus
            {
                UserId = userId,
                NotificationId = notificationId,
                Dismissed = true,
                DismissedAt = DateTime.UtcNow
            };
            _context.UserNotificationStatuses.Add(status);
        }
        else
        {
            status.Dismissed = true;
            status.DismissedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}