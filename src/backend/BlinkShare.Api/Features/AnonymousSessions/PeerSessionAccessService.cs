using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Features.AnonymousSessions.CreateOrJoin;
using BlinkShare.Api.Infrastructure.Redis;

namespace BlinkShare.Api.Features.AnonymousSessions;

public sealed class PeerSessionAccessService(
    IPeerSessionStore peerSessionStore,
    IClock clock)
{
    public async Task<Result<AuthorizedPeerSession>> ValidateAsync(
        Guid sessionId,
        Guid peerId,
        string? resumeToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resumeToken))
        {
            return Result<AuthorizedPeerSession>.Failure(Errors.General.Validation("Resume token is required."));
        }

        var session = await peerSessionStore.GetBySessionIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            return Result<AuthorizedPeerSession>.Failure(Errors.Session.NotFound());
        }

        if (session.IsExpired(clock.UtcNow))
        {
            return Result<AuthorizedPeerSession>.Failure(Errors.Session.Expired());
        }

        var peer = session.FindPeer(peerId);
        if (peer is null)
        {
            return Result<AuthorizedPeerSession>.Failure(Errors.Session.PeerNotFound());
        }

        if (!string.Equals(peer.ResumeTokenHash, CreateOrJoin.Handler.HashToken(resumeToken), StringComparison.Ordinal))
        {
            return Result<AuthorizedPeerSession>.Failure(Errors.Session.InvalidResumeToken());
        }

        if (!peer.IsReconnectValid(clock.UtcNow))
        {
            return Result<AuthorizedPeerSession>.Failure(Errors.Session.ReconnectGraceElapsed());
        }

        return Result<AuthorizedPeerSession>.Success(new AuthorizedPeerSession(session, peer));
    }

    public async Task<PeerSessionState> TouchAsync(
        PeerSessionState session,
        Guid peerId,
        CancellationToken cancellationToken)
    {
        var updatedSession = session.TouchPeer(peerId, clock.UtcNow);
        await peerSessionStore.SaveAsync(updatedSession, cancellationToken);
        return updatedSession;
    }
}

public sealed record AuthorizedPeerSession(
    PeerSessionState Session,
    PeerState Peer);
