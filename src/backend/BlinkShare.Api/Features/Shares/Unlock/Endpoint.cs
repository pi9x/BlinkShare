using BlinkShare.Api.Common.Endpoints;
using BlinkShare.Api.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace BlinkShare.Api.Features.Shares.Unlock;

public sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/shares/{code}/unlock", Handle)
            .WithTags("Shares")
            .WithName("UnlockShare")
            .WithSummary("Validates a passcode for a protected share.")
            .WithDescription("Returns a short-lived unlock proof for a protected share when the passcode is valid.")
            .Accepts<Request>("application/json")
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status410Gone)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Handle(
        string code,
        Request request,
        [FromServices] Handler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(code, request.Passcode, cancellationToken);

        return result.ToHttpResult(TypedResults.Ok);
    }
}
