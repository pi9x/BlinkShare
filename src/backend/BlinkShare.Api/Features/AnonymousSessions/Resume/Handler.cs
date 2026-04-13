using BlinkShare.Api.Common.Results;

namespace BlinkShare.Api.Features.AnonymousSessions.Resume;

public sealed class Handler(
    PeerSessionAccessService accessService) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public async Task<Result<Response>> HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var accessResult = await accessService.ValidateAsync(
            request.SessionId,
            request.PeerId,
            request.ResumeToken,
            cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result<Response>.Failure(accessResult.Error!);
        }

        var updatedSession = await accessService.TouchAsync(accessResult.Value!.Session, request.PeerId, cancellationToken);

        return Result<Response>.Success(new Response(
            updatedSession.SessionId,
            updatedSession.Code,
            request.PeerId,
            updatedSession.PeerCount,
            updatedSession.LastActivityAtUtc));
    }
}
