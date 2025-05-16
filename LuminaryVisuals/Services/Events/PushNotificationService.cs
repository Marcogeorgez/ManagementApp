using LuminaryVisuals.Data;
using LuminaryVisuals.Data.Entities;
using LuminaryVisuals.Utility;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WebPush;

public partial class PushNotificationService
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
            var chatMessageReadStatus = await context.ChatReadStatus.FirstOrDefaultAsync(s => s.UserId == user.Id && s.MessageId == chatMessageId);
            if (chatMessageReadStatus != null && chatMessageReadStatus.IsRead)
            {
                return;
            }
            var chatMessage = await context.Messages.FirstOrDefaultAsync(s => s.MessageId == chatMessageId);
            if (chatMessage != null)
            {
                message = $"{chatMessage.Content}";
            }
        }
        var vapidDetails = new VapidDetails("mailto:management@luminaryvisuals.net", _publicKey, _privateKey);
        var webPushClient = new WebPushClient();
        var cleanedMessage = CleanText(message);

        var payload = new
        {
            title,
            message = cleanedMessage
        };

        var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);

        try
        {
            foreach (var item in userNotification)
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
    public static string CleanText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        // Replace <img> tags with (Image)
        string cleanedText = ImageTagRegex().Replace(text, "(Image)");
        // Remove all HTML tags
        cleanedText = LineBreakRegex().Replace(text, "\n");
        cleanedText = MyRegexes.HtmlCleanerRegex().Replace(text, " ");

        cleanedText = System.Net.WebUtility.HtmlDecode(cleanedText);

        return cleanedText.Trim();
    }


    [GeneratedRegex(@"<br\s*/?>", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex LineBreakRegex();
    [GeneratedRegex("<img[^>]*>", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ImageTagRegex();
}
