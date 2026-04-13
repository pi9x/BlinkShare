using BlinkShare.Api.Common.Endpoints;
using BlinkShare.Api.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace BlinkShare.Api.Features.AnonymousSessions.PublishFileMetadata;

public sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/anonymous-sessions/{sessionId:guid}/file-metadata", Handle)
            .WithTags("AnonymousSessions")
            .WithName("PublishAnonymousSessionFileMetadata")
            .WithSummary("Publishes file metadata into an anonymous peer session.")
            .WithDescription("Validates the calling peer and returns the relay payload that connected peers can use.")
            .Accepts<Request>("application/json")
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status410Gone)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> Handle(
        Guid sessionId,
        Request request,
        [FromServices] Handler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request.ToCommand(sessionId), cancellationToken);

        return result.ToHttpResult(TypedResults.Ok);
    }
}
