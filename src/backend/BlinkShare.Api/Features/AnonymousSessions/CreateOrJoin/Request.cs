namespace BlinkShare.Api.Features.AnonymousSessions.CreateOrJoin;

public sealed record Request(string? Code)
{
    public Command ToCommand() => new(Code);
}
