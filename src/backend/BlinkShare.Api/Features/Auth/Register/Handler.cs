using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.Auth;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Features.Auth.Register;

public sealed class Handler(
    IDbContextFactory<BlinkShareDbContext> dbContextFactory,
    Validator validator,
    IAccountPasswordHasher passwordHasher,
    IAuthSessionTokenFactory authSessionTokenFactory,
    IClock clock,
    IOptions<AuthSessionOptions> options) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(command);
        if (validationResult.IsFailure)
        {
            return Result<Response>.Failure(validationResult.Error!);
        }

        var normalizedEmail = NormalizeEmail(command.Email!);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var exists = await dbContext.Accounts
            .AsNoTracking()
            .AnyAsync(account => account.NormalizedEmail == normalizedEmail, cancellationToken);

        if (exists)
        {
            return Result<Response>.Failure(Errors.Auth.DuplicateEmail());
        }

        var now = clock.UtcNow;
        var account = new Account(
            Guid.NewGuid(),
            command.Email!.Trim(),
            normalizedEmail,
            passwordHasher.Hash(command.Password!),
            now,
            now);

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

        await dbContext.Accounts.AddAsync(account, cancellationToken);
        await dbContext.AuthSessions.AddAsync(authSession, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<Response>.Success(new Response(
            account.Id,
            account.Email,
            sessionToken,
            sessionExpiresAtUtc));
    }

    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
