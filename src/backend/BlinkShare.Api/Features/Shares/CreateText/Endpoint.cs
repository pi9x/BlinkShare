using BlinkShare.Api.Common.Endpoints;
using BlinkShare.Api.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace BlinkShare.Api.Features.Shares.CreateText;

public sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/shares/text", Handle)
            .WithTags("Shares")
            .WithName("CreateTextShare")
            .WithSummary("Creates a stored text share.")
            .WithDescription("Creates a stored text share for the free tier and returns its identifier, code, and expiry.")
            .Accepts<Request>("application/json")
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Handle(
        Request request,
        [FromServices] Handler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request.ToCommand(), cancellationToken);

        return result.ToHttpResult(response => TypedResults.Created($"/api/v1/shares/{response.Code}", response));
    }
}
