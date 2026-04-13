namespace BlinkShare.Api.Features.Auth.Register;

public sealed record Request(string? Email, string? Password)
{
    public Command ToCommand() => new(Email, Password);
}
