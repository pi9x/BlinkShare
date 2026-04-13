namespace BlinkShare.Worker.Features.CleanupObjects;

public sealed class WorkerOptions
{
    public int IntervalSeconds { get; set; } = 30;

    public int BatchSize { get; set; } = 100;
}
