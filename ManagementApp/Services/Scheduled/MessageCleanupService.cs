using ManagementApp.Data;

namespace ManagementApp.Services.Scheduled;

public interface IMessageCleanupService
{
    Task DeleteOldMessagesAsync();
}

public class MessageCleanupService : IMessageCleanupService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MessageCleanupService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task DeleteOldMessagesAsync()
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cutoffDate = DateTime.UtcNow.AddMonths(-4);
            var messagesToDelete = context.Messages
                .Where(m => m.Timestamp < cutoffDate)
                .ToList();

            if (messagesToDelete.Any())
            {
                context.Messages.RemoveRange(messagesToDelete);
                await context.SaveChangesAsync();
            }
        }
    }
}


