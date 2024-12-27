namespace LuminaryVisuals.Services.Scheduled;

public class MessageCleanupBackgroundService : BackgroundService
{
    private readonly IMessageCleanupService _messageCleanupService;
    private readonly ILogger<MessageCleanupBackgroundService> _logger;

    public MessageCleanupBackgroundService(IMessageCleanupService messageCleanupService, ILogger<MessageCleanupBackgroundService> logger)
    {
        _messageCleanupService = messageCleanupService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run every 7 days
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting weekly message cleanup...");
                await _messageCleanupService.DeleteOldMessagesAsync();
                _logger.LogInformation("Weekly message cleanup completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred during message cleanup: {ex.Message}");
            }

            // Wait for 1 week before running the cleanup again
            await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
        }
    }
}
