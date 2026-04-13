namespace BlinkShare.Api.Infrastructure.Auth;

public interface IAuthSessionTokenFactory
{
    string CreateToken();
}
