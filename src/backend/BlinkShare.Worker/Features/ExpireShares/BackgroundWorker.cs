using Microsoft.Extensions.Options;

namespace BlinkShare.Worker.Features.ExpireShares;

public sealed class BackgroundWorker(
    Handler handler,
    IOptions<WorkerOptions> options,
    ILogger<BackgroundWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.Value.IntervalSeconds));

        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            var expiredCount = await handler.HandleAsync(stoppingToken);
            logger.LogInformation("Expired {ExpiredCount} shares.", expiredCount);
        }
    }
}
