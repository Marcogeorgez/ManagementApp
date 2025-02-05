using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Services;
using LuminaryVisuals.Services.Events;
using LuminaryVisuals.Services.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

public class UserNotificationService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public UserNotificationService(IDbContextFactory<ApplicationDbContext>  context)
    {
        _contextFactory = context;
    }

    // Creates a new notification
    public async Task CreateNotification(string message, string? targetRole)
    {
        using var _context = _contextFactory.CreateDbContext();

        var notification = new Notification
        {
            Message = message,
            CreatedAt = DateTime.UtcNow,
            TargetRole = targetRole
        };
        if(notification.TargetRole == "Everyone")
        {
            notification.TargetRole = null;
        }

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    // Get active notifications for a user
    public async Task<List<Notification>> GetActiveNotificationsForUser(string userId, string userRole)
    {
        using var _context = _contextFactory.CreateDbContext();
        return await _context.Notifications
            .Where(n =>
            // Check if notification is for all users (TargetRole is null) 
            // OR if it matches the user's role
            ( n.TargetRole == null || n.TargetRole == userRole )
            && !n.UserNotificationStatuses
                .Any(uns => uns.UserId == userId && uns.Dismissed))
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