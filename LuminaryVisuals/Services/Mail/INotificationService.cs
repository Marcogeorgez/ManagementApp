
using LuminaryVisuals.Data.Entities;

namespace LuminaryVisuals.Services.Mail
{
    public interface INotificationService
    {
        Task QueueChatNotification(Project project, Message message);
        Task QueueProjectCreationNotification(Project project);
        Task QueueStatusChangeNotification(Project project, ProjectStatus oldStatus, ProjectStatus newStatus);
    }
}