using System.Security.Cryptography;
using System.Text;
using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.Redis;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Features.AnonymousSessions.CreateOrJoin;

public sealed class Handler(
    IPeerSessionStore peerSessionStore,
    Validator validator,
    IClock clock,
    ICodeGenerator codeGenerator,
    IOptions<AnonymousSessionOptions> options) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    private const int MaxCodeGenerationAttempts = 5;

    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(command);
        if (validationResult.IsFailure)
        {
            return Result<Response>.Failure(validationResult.Error!);
        }

        if (string.IsNullOrWhiteSpace(command.Code))
        {
            return await CreateSessionAsync(cancellationToken);
        }

        return await JoinSessionAsync(command.Code, cancellationToken);
    }

    private async Task<Result<Response>> CreateSessionAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxCodeGenerationAttempts; attempt++)
        {
            var code = codeGenerator.GenerateShareCode();
            var existing = await peerSessionStore.GetByCodeAsync(code, cancellationToken);

            if (existing is not null)
            {
                continue;
            }

            var now = clock.UtcNow;
            var peerId = Guid.NewGuid();
            var resumeToken = CreateToken();
            var peer = new PeerState(
                peerId,
                HashToken(resumeToken),
                now,
                now,
                now.AddSeconds(options.Value.ReconnectGraceSeconds));
            var session = new PeerSessionState(
                Guid.NewGuid(),
                code,
                PeerSessionStatus.Waiting,
                now,
                now.AddMinutes(options.Value.SessionLifetimeMinutes),
                now,
                options.Value.ReconnectGraceSeconds,
                [peer]);

            await peerSessionStore.CreateAsync(session, cancellationToken);

            return Result<Response>.Success(new Response(
                session.SessionId,
                session.Code,
                peer.PeerId,
                resumeToken,
                session.PeerCount,
                session.ReconnectGraceSeconds));
        }

        return Result<Response>.Failure(Errors.Share.CodeUnavailable("A unique peer session code could not be generated."));
    }

    private async Task<Result<Response>> JoinSessionAsync(string code, CancellationToken cancellationToken)
    {
        var session = await peerSessionStore.GetByCodeAsync(code, cancellationToken);
        if (session is null)
        {
            return Result<Response>.Failure(Errors.Session.NotFound());
        }

        if (session.IsExpired(clock.UtcNow))
        {
            return Result<Response>.Failure(Errors.Session.Expired());
        }

        var now = clock.UtcNow;
        var peerId = Guid.NewGuid();
        var resumeToken = CreateToken();
        var peer = new PeerState(
            peerId,
            HashToken(resumeToken),
            now,
            now,
            now.AddSeconds(session.ReconnectGraceSeconds));
        var updatedSession = session.AddPeer(peer, now);

        await peerSessionStore.SaveAsync(updatedSession, cancellationToken);

        return Result<Response>.Success(new Response(
            updatedSession.SessionId,
            updatedSession.Code,
            peer.PeerId,
            resumeToken,
            updatedSession.PeerCount,
            updatedSession.ReconnectGraceSeconds));
    }

    private static string CreateToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    internal static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
