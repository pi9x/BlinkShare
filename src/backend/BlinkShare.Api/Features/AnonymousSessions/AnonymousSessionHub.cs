using BlinkShare.Api.Common.Results;
using Microsoft.AspNetCore.SignalR;

namespace BlinkShare.Api.Features.AnonymousSessions;

public sealed class AnonymousSessionHub(
    PeerSessionAccessService accessService,
    PublishText.Validator publishTextValidator,
    PublishFileMetadata.Validator publishFileMetadataValidator)
    : Hub
{
    private const string SessionIdItemKey = "AnonymousSession.SessionId";
    private const string PeerIdItemKey = "AnonymousSession.PeerId";

    public async Task ConnectSession(
        Guid sessionId,
        Guid peerId,
        string? resumeToken)
    {
        var cancellationToken = Context.ConnectionAborted;
        var accessResult = await accessService.ValidateAsync(sessionId, peerId, resumeToken, cancellationToken);
        if (accessResult.IsFailure)
        {
            throw CreateHubException(accessResult.Error!);
        }

        var updatedSession = await accessService.TouchAsync(accessResult.Value!.Session, peerId, cancellationToken);
        Context.Items[SessionIdItemKey] = sessionId;
        Context.Items[PeerIdItemKey] = peerId;
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
        string? text)
    {
        var cancellationToken = Context.ConnectionAborted;
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
        string? shareCode = null)
    {
        var cancellationToken = Context.ConnectionAborted;
        var validationResult = publishFileMetadataValidator.Validate(
            new PublishFileMetadata.Command(sessionId, peerId, resumeToken, fileName, contentType, sizeBytes, shareCode));
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
            NormalizeShareCode(shareCode),
            updatedSession.LastActivityAtUtc);

        await Clients.Group(GetGroupName(sessionId)).SendAsync("contentFileMetadataReceived", message, cancellationToken);

        return message;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (TryGetConnectionSession(out var sessionId, out var peerId))
        {
            var updatedSession = await accessService.MarkDisconnectedAsync(sessionId, peerId, CancellationToken.None);
            if (updatedSession is not null)
            {
                await Clients.Group(GetGroupName(sessionId)).SendAsync(
                    "peerConnected",
                    new PeerConnectedMessage(updatedSession.SessionId, peerId, updatedSession.PeerCount),
                    CancellationToken.None);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static string GetGroupName(Guid sessionId) => $"anonymous-session:{sessionId:D}";

    private static string? NormalizeShareCode(string? shareCode) =>
        string.IsNullOrWhiteSpace(shareCode) ? null : shareCode.Trim().ToUpperInvariant();

    private static HubException CreateHubException(Error error) =>
        new($"{error.Code}|{error.Message}");

    private bool TryGetConnectionSession(out Guid sessionId, out Guid peerId)
    {
        sessionId = Guid.Empty;
        peerId = Guid.Empty;

        if (!Context.Items.TryGetValue(SessionIdItemKey, out var sessionValue) || sessionValue is not Guid storedSessionId)
        {
            return false;
        }

        if (!Context.Items.TryGetValue(PeerIdItemKey, out var peerValue) || peerValue is not Guid storedPeerId)
        {
            return false;
        }

        sessionId = storedSessionId;
        peerId = storedPeerId;
        return true;
    }
}
