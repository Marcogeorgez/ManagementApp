namespace ManagementApp.Data.Entities;

public class UserNotificationStatus
{
    public string UserId { get; set; }
    public int NotificationId { get; set; }
    public bool Dismissed { get; set; }
    public DateTime? DismissedAt { get; set; }

    // Navigation properties
    public Notification Notification { get; set; }
}
