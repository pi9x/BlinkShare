namespace BlinkShare.Api.IntegrationTests.Infrastructure;

internal static class TestPaths
{
    public static string GetApiContentRoot()
    {
        var contentRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "BlinkShare.Api"));

        if (Directory.Exists(contentRoot))
        {
            return contentRoot;
        }

        throw new DirectoryNotFoundException(
            $"Could not resolve the BlinkShare.Api content root from '{AppContext.BaseDirectory}'.");
    }
}
