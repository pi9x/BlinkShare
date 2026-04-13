namespace BlinkShare.Api.Common.Auth;

public interface ICurrentAccountAccessor
{
    CurrentAccount? GetCurrentAccount();
}
