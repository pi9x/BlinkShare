using BlinkShare.Api.Common.Auth;
using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlinkShare.Api.Features.Auth.Me;

public sealed class Handler(
    IDbContextFactory<BlinkShareDbContext> dbContextFactory,
    ICurrentAccountAccessor currentAccountAccessor) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public async Task<Result<Response>> HandleAsync(CancellationToken cancellationToken)
    {
        var currentAccount = currentAccountAccessor.GetCurrentAccount();
        if (currentAccount is null)
        {
            return Result<Response>.Failure(Errors.Auth.Unauthorized());
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var response = await dbContext.Accounts
            .Where(account => account.Id == currentAccount.AccountId)
            .Select(account => new Response(account.Id, account.Email, account.Tier, account.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

        return response is null
            ? Result<Response>.Failure(Errors.Auth.Unauthorized())
            : Result<Response>.Success(response);
    }
}
