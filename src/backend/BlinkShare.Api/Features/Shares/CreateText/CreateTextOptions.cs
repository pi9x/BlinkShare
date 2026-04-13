namespace BlinkShare.Api.Features.Shares.CreateText;

public sealed class CreateTextOptions
{
    public const string SectionName = "Shares:CreateText";

    public int MaxTextLength { get; init; } = 10_000;

    public int FreeTierTtlMinutes { get; init; } = 5;
}
