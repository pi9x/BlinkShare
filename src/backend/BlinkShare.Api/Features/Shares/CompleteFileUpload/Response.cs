using BlinkShare.Api.Infrastructure.Persistence;

namespace BlinkShare.Api.Features.Shares.CompleteFileUpload;

public sealed record Response(
    Guid ShareId,
    string Code,
    ShareStatus Status);
