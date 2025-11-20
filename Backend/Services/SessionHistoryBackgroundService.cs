using HDScheduler.API.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HDScheduler.API.Services;

public class SessionHistoryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SessionHistoryBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Check every 5 minutes

    public SessionHistoryBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<SessionHistoryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session History Background Service is starting.");

        // Wait a bit before starting the first check
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MoveCompletedSessionsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while moving completed sessions to history.");
            }

            // Wait for the next check interval
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Session History Background Service is stopping.");
    }

    private async Task MoveCompletedSessionsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var hdScheduleRepository = scope.ServiceProvider.GetRequiredService<IHDScheduleRepository>();

        _logger.LogInformation("Checking for completed sessions to move to history...");
        
        var moved = await hdScheduleRepository.MoveCompletedSessionsToHistoryAsync();
        
        if (moved)
        {
            _logger.LogInformation("Completed sessions moved to history successfully.");
        }
    }
}
