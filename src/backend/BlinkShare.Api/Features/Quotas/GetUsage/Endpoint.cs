using BlinkShare.Api.Common.Endpoints;
using BlinkShare.Api.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace BlinkShare.Api.Features.Quotas.GetUsage;

public sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/quotas/usage", Handle)
            .WithTags("Quotas")
            .WithName("GetQuotaUsage")
            .WithSummary("Returns current quota usage information.")
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Handle(
        [FromServices] Handler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(cancellationToken);
        return result.ToHttpResult(TypedResults.Ok);
    }
}
