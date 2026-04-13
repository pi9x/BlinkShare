using BlinkShare.Api.Common.Endpoints;
using BlinkShare.Api.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace BlinkShare.Api.Features.Shares.GetByCode;

public sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/shares/{code}", Handle)
            .WithTags("Shares")
            .WithName("GetShareByCode")
            .WithSummary("Gets public metadata for a share by code.")
            .WithDescription("Returns public metadata for a share without exposing stored text content.")
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status410Gone)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Handle(
        string code,
        [FromServices] Handler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(code, cancellationToken);

        return result.ToHttpResult(TypedResults.Ok);
    }
}
