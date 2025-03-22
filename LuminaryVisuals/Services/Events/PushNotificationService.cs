using LuminaryVisuals.Data.Entities;
using System;
using System.Threading.Tasks;
using WebPush;

public class PushNotificationService
{
    private readonly string _publicKey = "BEj-Wiu59-OGKk2V4EbpdKX3V6ODV7JSaBj_rkjfvSXpJQsAtvSmgyjWyOWkF1RC6F5VtBSCquFDs6w7EmZ4J80";
    private readonly string _privateKey = "eYN89bnSaq1aEBr1I5g14mI30AIHVDvJoeW3QpG-sE0";

    public async Task SendNotification(ApplicationUser user, string title, string message)
    {
        if (string.IsNullOrEmpty(user.PushEndpoint))
            return;

        var pushSubscription = new PushSubscription(user.PushEndpoint, user.PushP256DH, user.PushAuth);
        var vapidDetails = new VapidDetails("mailto:your@email.com", _publicKey, _privateKey);
        var webPushClient = new WebPushClient();

        var payload = new
        {
            title,
            message
        };

        var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);

        try
        {
            await webPushClient.SendNotificationAsync(pushSubscription, jsonPayload, vapidDetails);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending notification: {ex.Message}");
        }
    }
}
