using System.Security.Claims;
using BlinkShare.Api.Infrastructure.Persistence;
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
        var tierValue = user.FindFirstValue(BlinkShareAuthConstants.AccountTierClaimType);

        if (!Guid.TryParse(accountIdValue, out var accountId) ||
            string.IsNullOrWhiteSpace(email) ||
            !Enum.TryParse<AccountTier>(tierValue, ignoreCase: true, out var tier))
        {
            return null;
        }

        return new CurrentAccount(accountId, email, tier);
    }
}
