using BlinkShare.Api.Common.Results;
using Microsoft.AspNetCore.SignalR;

namespace BlinkShare.Api.Features.AnonymousSessions;

public sealed class AnonymousSessionHub(
    PeerSessionAccessService accessService,
    PublishText.Validator publishTextValidator,
    PublishFileMetadata.Validator publishFileMetadataValidator)
    : Hub
{
    public async Task ConnectSession(
        Guid sessionId,
        Guid peerId,
        string? resumeToken,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.ValidateAsync(sessionId, peerId, resumeToken, cancellationToken);
        if (accessResult.IsFailure)
        {
            throw CreateHubException(accessResult.Error!);
        }

        var updatedSession = await accessService.TouchAsync(accessResult.Value!.Session, peerId, cancellationToken);
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(sessionId), cancellationToken);

        await Clients.Group(GetGroupName(sessionId)).SendAsync(
            "peerConnected",
            new PeerConnectedMessage(updatedSession.SessionId, peerId, updatedSession.PeerCount),
            cancellationToken);
    }

    public async Task<TextPublishedMessage> PublishText(
        Guid sessionId,
        Guid peerId,
        string? resumeToken,
        string? text,
        CancellationToken cancellationToken)
    {
        var validationResult = publishTextValidator.Validate(new PublishText.Command(sessionId, peerId, resumeToken, text));
        if (validationResult.IsFailure)
        {
            throw CreateHubException(validationResult.Error!);
        }

        var accessResult = await accessService.ValidateAsync(sessionId, peerId, resumeToken, cancellationToken);
        if (accessResult.IsFailure)
        {
            throw CreateHubException(accessResult.Error!);
        }

        var updatedSession = await accessService.TouchAsync(accessResult.Value!.Session, peerId, cancellationToken);
        var message = new TextPublishedMessage(updatedSession.SessionId, peerId, updatedSession.LastActivityAtUtc, text!);

        await Clients.Group(GetGroupName(sessionId)).SendAsync("contentTextReceived", message, cancellationToken);

        return message;
    }

    public async Task<FileMetadataPublishedMessage> PublishFileMetadata(
        Guid sessionId,
        Guid peerId,
        string? resumeToken,
        string? fileName,
        string? contentType,
        long sizeBytes,
        CancellationToken cancellationToken)
    {
        var validationResult = publishFileMetadataValidator.Validate(
            new PublishFileMetadata.Command(sessionId, peerId, resumeToken, fileName, contentType, sizeBytes));
        if (validationResult.IsFailure)
        {
            throw CreateHubException(validationResult.Error!);
        }

        var accessResult = await accessService.ValidateAsync(sessionId, peerId, resumeToken, cancellationToken);
        if (accessResult.IsFailure)
        {
            throw CreateHubException(accessResult.Error!);
        }

        var updatedSession = await accessService.TouchAsync(accessResult.Value!.Session, peerId, cancellationToken);
        var message = new FileMetadataPublishedMessage(
            updatedSession.SessionId,
            peerId,
            fileName!,
            contentType!,
            sizeBytes,
            updatedSession.LastActivityAtUtc);

        await Clients.Group(GetGroupName(sessionId)).SendAsync("contentFileMetadataReceived", message, cancellationToken);

        return message;
    }

    private static string GetGroupName(Guid sessionId) => $"anonymous-session:{sessionId:D}";

    private static HubException CreateHubException(Error error) =>
        new($"{error.Code}|{error.Message}");
}
