using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.Auth;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Features.Auth.Login;

public sealed class Handler(
    IDbContextFactory<BlinkShareDbContext> dbContextFactory,
    IAccountPasswordHasher passwordHasher,
    IAuthSessionTokenFactory authSessionTokenFactory,
    IClock clock,
    IOptions<AuthSessionOptions> options) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public async Task<Result<Response>> HandleAsync(Request request, CancellationToken cancellationToken)
    {
        if (request.Email is null)
        {
            return Result<Response>.Failure(Errors.General.Validation("Email is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<Response>.Failure(Errors.General.Validation("Email cannot be blank."));
        }

        if (request.Password is null)
        {
            return Result<Response>.Failure(Errors.General.Validation("Password is required."));
        }

        var normalizedEmail = Register.Handler.NormalizeEmail(request.Email);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var account = await dbContext.Accounts
            .SingleOrDefaultAsync(candidate => candidate.NormalizedEmail == normalizedEmail, cancellationToken);

        if (account is null || !passwordHasher.Verify(request.Password, account.PasswordHash))
        {
            return Result<Response>.Failure(Errors.Auth.InvalidCredentials());
        }

        var now = clock.UtcNow;
        account.MarkLoggedIn(now);

        var sessionToken = authSessionTokenFactory.CreateToken();
        var sessionExpiresAtUtc = now.AddHours(options.Value.SessionLifetimeHours);
        var authSession = new AuthSession(
            Guid.NewGuid(),
            account.Id,
            AuthSessionTokenHasher.Hash(sessionToken),
            now,
            sessionExpiresAtUtc,
            now,
            null);

        await dbContext.AuthSessions.AddAsync(authSession, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<Response>.Success(new Response(
            account.Id,
            account.Email,
            sessionToken,
            sessionExpiresAtUtc));
    }
}
