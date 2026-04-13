using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BlinkShare.Api.Common.Auth;

public sealed class HttpContextCurrentAccountAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentAccountAccessor
{
    public CurrentAccount? GetCurrentAccount()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var accountIdValue = user.FindFirstValue(BlinkShareAuthConstants.AccountIdClaimType);
        var email = user.FindFirstValue(BlinkShareAuthConstants.AccountEmailClaimType);

        if (!Guid.TryParse(accountIdValue, out var accountId) || string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return new CurrentAccount(accountId, email);
    }
}
