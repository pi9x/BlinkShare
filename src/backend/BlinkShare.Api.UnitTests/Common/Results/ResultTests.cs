using BlinkShare.Api.Common.Results;

namespace BlinkShare.Api.UnitTests.Common.Results;

public sealed class ResultTests
{
    [Fact]
    public void Failure_requires_an_error()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new InvalidResult());

        Assert.Equal("error", exception.ParamName);
    }

    [Fact]
    public void Success_sets_expected_state()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Generic_failure_carries_the_error()
    {
        var error = Errors.Share.InvalidText();
        var result = Result<string>.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Same(error, result.Error);
        Assert.Null(result.Value);
    }

    private sealed class InvalidResult : Result
    {
        public InvalidResult()
            : base(false, null)
        {
        }
    }
}
