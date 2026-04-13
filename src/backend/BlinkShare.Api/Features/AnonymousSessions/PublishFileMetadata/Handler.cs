using BlinkShare.Api.Common.Results;

namespace BlinkShare.Api.Features.AnonymousSessions.PublishFileMetadata;

public sealed class Handler(
    PeerSessionAccessService accessService,
    Validator validator) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(command);
        if (validationResult.IsFailure)
        {
            return Result<Response>.Failure(validationResult.Error!);
        }

        var accessResult = await accessService.ValidateAsync(
            command.SessionId,
            command.PeerId,
            command.ResumeToken,
            cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result<Response>.Failure(accessResult.Error!);
        }

        var updatedSession = await accessService.TouchAsync(accessResult.Value!.Session, command.PeerId, cancellationToken);

        return Result<Response>.Success(new Response(
            updatedSession.SessionId,
            command.PeerId,
            command.FileName!,
            command.ContentType!,
            command.SizeBytes,
            updatedSession.LastActivityAtUtc));
    }
}
