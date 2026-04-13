using BlinkShare.Api.Common.Auth;
using BlinkShare.Api.Common.Endpoints;
using BlinkShare.Api.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace BlinkShare.Api.Features.Shares.RequestDownload;

public sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/shares/{code}/download", Handle)
            .WithTags("Shares")
            .WithName("RequestShareDownload")
            .WithSummary("Requests a signed download target for a file share.")
            .WithDescription("Returns a download URL for a ready file share and updates its access counters.")
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status410Gone)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Handle(
        string code,
        [FromHeader(Name = ShareUnlockHeaders.ProofHeaderName)] string? unlockProof,
        [FromServices] Handler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(code, unlockProof, cancellationToken);

        return result.ToHttpResult(TypedResults.Ok);
    }
}
