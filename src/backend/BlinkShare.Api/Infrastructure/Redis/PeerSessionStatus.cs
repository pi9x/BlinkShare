namespace BlinkShare.Api.Infrastructure.Redis;

public enum PeerSessionStatus
{
    Waiting = 1,
    Active = 2,
    Reconnecting = 3,
    Expired = 4,
    Closed = 5
}
