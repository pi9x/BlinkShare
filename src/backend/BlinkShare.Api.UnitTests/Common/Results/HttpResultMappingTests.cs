using BlinkShare.Api.Common.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BlinkShare.Api.UnitTests.Common.Results;

public sealed class HttpResultMappingTests
{
    [Fact]
    public void ToHttpResult_returns_404_for_not_found_errors()
    {
        var result = Result.Failure(Errors.Share.NotFound());

        var httpResult = result.ToHttpResult();

        var statusCodeResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(httpResult);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
    }

    [Fact]
    public void ToHttpResult_returns_200_for_successful_generic_results()
    {
        var result = Result<string>.Success("blinkshare");

        var httpResult = result.ToHttpResult();

        var okResult = Assert.IsType<Ok<string>>(httpResult);
        Assert.Equal("blinkshare", okResult.Value);
    }

    [Fact]
    public void ToHttpResult_uses_custom_success_mapper_for_generic_results()
    {
        var result = Result<Guid>.Success(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"));

        var httpResult = result.ToHttpResult(value => TypedResults.Created($"/shares/{value}"));

        var createdResult = Assert.IsType<Created>(httpResult);
        Assert.Equal("/shares/aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", createdResult.Location);
    }
}
