using System.Security.Claims;
using System.Text.Encodings.Web;
using BlinkShare.Api.Infrastructure.Auth;
using BlinkShare.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Common.Auth;

public sealed class BlinkShareBearerAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IDbContextFactory<BlinkShareDbContext> dbContextFactory)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = Request.Headers.Authorization.ToString()["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return AuthenticateResult.Fail("Bearer token is required.");
        }

        var tokenHash = AuthSessionTokenHasher.Hash(token);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(Context.RequestAborted);

        var account = await dbContext.AuthSessions
            .AsNoTracking()
            .Where(session => session.TokenHash == tokenHash)
            .Where(session => session.RevokedAtUtc == null && session.ExpiresAtUtc > DateTimeOffset.UtcNow)
            .Join(
                dbContext.Accounts.AsNoTracking(),
                session => session.AccountId,
                account => account.Id,
                (session, account) => new
                {
                    account.Id,
                    account.Email
                })
            .SingleOrDefaultAsync(Context.RequestAborted);

        if (account is null)
        {
            return AuthenticateResult.Fail("Bearer token is invalid.");
        }

        var claims = new List<Claim>
        {
            new(BlinkShareAuthConstants.AccountIdClaimType, account.Id.ToString("D")),
            new(BlinkShareAuthConstants.AccountEmailClaimType, account.Email)
        };

        var identity = new ClaimsIdentity(claims, BlinkShareAuthConstants.SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, BlinkShareAuthConstants.SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}
