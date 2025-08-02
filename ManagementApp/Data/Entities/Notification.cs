using MudBlazor;

namespace ManagementApp.Data.Entities;

public class Notification
{
    public int Id { get; set; }
    public string Message { get; set; }
    public string? TargetRole { get; set; } // null means all users
    public Severity SeverityColor { get; set; } = Severity.Warning;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<UserNotificationStatus> UserNotificationStatuses { get; set; }
}
