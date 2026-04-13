namespace BlinkShare.Api.Infrastructure.Security;

public interface IAccountPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string persistedHash);
}
