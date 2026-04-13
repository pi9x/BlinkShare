namespace BlinkShare.Api.Common.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
