using BlinkShare.Api.Common.Auth;
using BlinkShare.Api.Common.Endpoints;
using BlinkShare.Api.Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlinkShare.Api.Features.Auth.Me;

public sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/auth/me", Handle)
            .WithTags("Auth")
            .WithName("GetCurrentAccount")
            .WithSummary("Returns the currently authenticated account.")
            .RequireAuthorization(new AuthorizeAttribute
            {
                AuthenticationSchemes = BlinkShareAuthConstants.SchemeName
            })
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
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
