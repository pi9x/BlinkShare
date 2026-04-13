using BlinkShare.Api.Common.Endpoints;
using BlinkShare.Api.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace BlinkShare.Api.Features.Auth.Register;

public sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/register", Handle)
            .WithTags("Auth")
            .WithName("RegisterAccount")
            .WithSummary("Registers a new free account.")
            .WithDescription("Creates an account and returns an initial opaque auth session token.")
            .Accepts<Request>("application/json")
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Handle(
        Request request,
        [FromServices] Handler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request.ToCommand(), cancellationToken);
        return result.ToHttpResult(response => TypedResults.Created("/api/v1/auth/me", response));
    }
}
