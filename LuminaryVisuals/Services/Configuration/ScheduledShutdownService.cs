namespace LuminaryVisuals.Services.Configuration;

public class ScheduledShutdownService : BackgroundService
{
    private readonly ILogger<ScheduledShutdownService> _logger;

    // Environment variable names
    private const string ShutdownTimeEnv = "APP_SHUTDOWN_UTC_TIME"; // e.g. "22:30" (24h format)
    private const string ShutdownIntervalDaysEnv = "APP_SHUTDOWN_INTERVAL_DAYS"; // e.g. "2", "5"

    private TimeSpan _shutdownTime;
    private int _shutdownIntervalDays;

    // To track last shutdown date
    private DateTime _lastShutdownDate;

    public ScheduledShutdownService(ILogger<ScheduledShutdownService> logger)
    {
        _logger = logger;

        // Read and parse env variables
        var timeEnv = Environment.GetEnvironmentVariable(ShutdownTimeEnv);
        var intervalEnv = Environment.GetEnvironmentVariable(ShutdownIntervalDaysEnv);

        if (!TimeSpan.TryParse(timeEnv, out _shutdownTime))
        {
            _logger.LogWarning($"Invalid or missing {ShutdownTimeEnv}, defaulting to 23:59 UTC.");
            _shutdownTime = new TimeSpan(23, 59, 0);
        }

        if (!int.TryParse(intervalEnv, out _shutdownIntervalDays) || _shutdownIntervalDays <= 0) // prevents infinite loop if it's equal or less than 0
        {
            _logger.LogWarning($"Invalid or missing {ShutdownIntervalDaysEnv}, defaulting to once every 7 days.");
            _shutdownIntervalDays = 7;
        }

        // Initialize last shutdown date to something in the past
        _lastShutdownDate = DateTime.UtcNow.Date;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"ScheduledShutdownService started. Will shutdown every {_shutdownIntervalDays} day(s) at {_shutdownTime} UTC.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var utcNow = DateTime.UtcNow;
                var todayShutdownDateTime = utcNow.Date + _shutdownTime;

                // Check if the shutdown time for today has passed
                bool isTimeToShutdown = utcNow >= todayShutdownDateTime;

                // Check if enough days have passed since last shutdown
                bool intervalPassed = ( utcNow.Date - _lastShutdownDate.Date ).TotalDays >= _shutdownIntervalDays;

                if (isTimeToShutdown && intervalPassed)
                {
                    _logger.LogInformation($"Shutdown time reached at {utcNow}. Exiting app for maintenance.");

                    _lastShutdownDate = utcNow.Date;

                    // Exit the app gracefully:
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ScheduledShutdownService.");
            }

            // Check every 1 minute, can be adjusted
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}