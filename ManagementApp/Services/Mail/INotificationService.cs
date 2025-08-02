using ManagementApp.Data.Entities;

namespace ManagementApp.Services.Mail;

public interface INotificationService
{
    Task ClientPreferencesUpdated(ApplicationUser User, string UserId, Dictionary<string, string> newClientPreferences);
    Task NewUserJoinedNoitifcation(ApplicationUser User);
    Task QueueChatNotification(Project project, Message message);
    Task QueuePrivateChatNotification(string userId, Message message, Chat chat);
    Task QueueProjectScheduleUpdated(Project project, string UserId);
    Task QueueProjectCreationNotification(Project project);
    Task QueueStatusChangeNotification(Project project, ProjectStatus oldStatus, ProjectStatus newStatus, string updatedByUserId);
}