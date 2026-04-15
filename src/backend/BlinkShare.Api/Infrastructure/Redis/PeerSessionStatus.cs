namespace BlinkShare.Api.Infrastructure.Redis;

public enum PeerSessionStatus
{
    Waiting = 1,
    Active = 2,
    Reconnecting = 3,
    Closed = 4
}
