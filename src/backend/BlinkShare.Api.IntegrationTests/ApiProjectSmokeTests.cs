namespace BlinkShare.Api.IntegrationTests;

public sealed class ApiProjectSmokeTests
{
    [Fact]
    public void Program_type_is_accessible()
    {
        Assert.Equal("BlinkShare.Api", typeof(global::Program).Assembly.GetName().Name);
    }
}
