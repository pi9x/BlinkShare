using BlinkShare.Api.Common.Endpoints;
using BlinkShare.Api.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace BlinkShare.Api.Features.Shares.CreateFileUpload;

public sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/shares/file", Handle)
            .WithTags("Shares")
            .WithName("CreateFileUploadShare")
            .WithSummary("Creates a pending file share and returns an upload target.")
            .WithDescription("Creates a pending stored file share for the free tier and returns an upload URL.")
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
