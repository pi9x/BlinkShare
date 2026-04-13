using BlinkShare.Api.Common.Auth;
using BlinkShare.Api.Common.Endpoints;
using BlinkShare.Api.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace BlinkShare.Api.Features.Shares.ReadText;

public sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/shares/{code}/text", Handle)
            .WithTags("Shares")
            .WithName("ReadTextShare")
            .WithSummary("Reads stored text share content.")
            .WithDescription("Returns stored text when the share exists, is not expired, and is accessible.")
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
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
