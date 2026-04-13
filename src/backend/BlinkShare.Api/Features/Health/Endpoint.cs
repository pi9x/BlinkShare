using BlinkShare.Api.Common.Endpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BlinkShare.Api.Features.Health;

public sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/healthz", Handle)
            .WithTags("Health")
            .WithName("GetHealth")
            .WithSummary("Returns a lightweight health response.");
    }

    private static Ok<Response> Handle() => TypedResults.Ok(new Response("ok"));

    private sealed record Response(string Status);
}
