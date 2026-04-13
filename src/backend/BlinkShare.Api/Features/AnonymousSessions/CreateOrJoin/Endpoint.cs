using BlinkShare.Api.Common.Endpoints;
using BlinkShare.Api.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace BlinkShare.Api.Features.AnonymousSessions.CreateOrJoin;

public sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/anonymous-sessions/join", Handle)
            .WithTags("AnonymousSessions")
            .WithName("CreateOrJoinAnonymousSession")
            .WithSummary("Creates a new anonymous peer session or joins an existing one.")
            .WithDescription("Creates a new peer session when no code is supplied, or joins an existing peer session by code.")
            .Accepts<Request>("application/json")
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status410Gone)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Handle(
        Request request,
        [FromServices] Handler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request.ToCommand(), cancellationToken);

        return result.ToHttpResult(TypedResults.Ok);
    }
}
