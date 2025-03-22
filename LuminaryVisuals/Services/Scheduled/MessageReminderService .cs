using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


public class MessageReminderService : BackgroundService
{
    private readonly IServiceProvider _services;

    public MessageReminderService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var pushService = scope.ServiceProvider.GetRequiredService<PushNotificationService>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var chatService = scope.ServiceProvider.GetRequiredService<ChatService>();
                // Get all users with push subscriptions
                var users = await dbContext.Users
                    .Where(u => !string.IsNullOrEmpty(u.PushEndpoint))
                    .ToListAsync();

                foreach (var user in users)
                {
                    var unreadCount = await chatService.GetUnreadMessageCount(user.Id);
                    if (unreadCount > 0)
                    {
                        await pushService.SendNotification(
                            user,
                            "Unread Messages",
                            $"You have {unreadCount} unread message(s)");
                        Console.WriteLine($"send message to {user.UserName}.");
                    }
                }
            }

            // Wait 5 minutes before next check
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}