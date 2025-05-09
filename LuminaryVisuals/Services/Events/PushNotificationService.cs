using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using Microsoft.EntityFrameworkCore;
using WebPush;

public class PushNotificationService
{
    private readonly string _publicKey = "BEj-Wiu59-OGKk2V4EbpdKX3V6ODV7JSaBj_rkjfvSXpJQsAtvSmgyjWyOWkF1RC6F5VtBSCquFDs6w7EmZ4J80";
    private readonly string _privateKey = "eYN89bnSaq1aEBr1I5g14mI30AIHVDvJoeW3QpG-sE0";
    private readonly IDbContextFactory<ApplicationDbContext> contextFactory;

    public PushNotificationService(IDbContextFactory<ApplicationDbContext> _contextFactory)
    {
        contextFactory = _contextFactory;
    }
    public async Task SendNotification(ApplicationUser user, string title, string message, int? chatMessageId)
    {
        using var context = contextFactory.CreateDbContext();
        var userNotification = await context.PushNotificationSubscriptions
            .Where(s => s.UserId == user.Id && s.Status == true)
            .ToListAsync();

        // Checks for if type of notification is a chat message, if so we check if user have read it or not
        // if not read it we send notification, else skip it
        if (chatMessageId != null)
        {
            var chatMessage = await context.ChatReadStatus.FirstOrDefaultAsync(s => s.UserId == user.Id && s.MessageId == chatMessageId);
            if (chatMessage == null)
                return;
            if (chatMessage.IsRead)
                return;
        }
        var vapidDetails = new VapidDetails("mailto:management@luminaryvisuals.net", _publicKey, _privateKey);
        var webPushClient = new WebPushClient();

        var payload = new
        {
            title,
            message
        };

        var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);

        try
        {
            foreach(var item in userNotification)
            {
                var pushSubscription = new PushSubscription(item.Endpoint, item.P256DH, item.Auth);
                await webPushClient.SendNotificationAsync(pushSubscription, jsonPayload, vapidDetails);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending notification: {ex.Message}");
        }
    }
}
