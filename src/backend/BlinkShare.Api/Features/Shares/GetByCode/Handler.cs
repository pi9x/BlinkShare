using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlinkShare.Api.Features.Shares.GetByCode;

public sealed class Handler(
    IDbContextFactory<BlinkShareDbContext> dbContextFactory,
    IClock clock) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public async Task<Result<Response>> HandleAsync(string code, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var response = await dbContext.Shares
            .AsNoTracking()
            .Where(share => share.Code == code)
            .Select(share => new Response(
                share.Id,
                share.Code,
                share.Kind,
                share.Status,
                share.ExpiresAtUtc,
                share.PasscodeHash != null,
                share.SizeBytes))
            .SingleOrDefaultAsync(cancellationToken);

        if (response is null)
        {
            return Result<Response>.Failure(Errors.Share.NotFound());
        }

        if (response.ExpiresAtUtc is { } expiresAtUtc && expiresAtUtc <= clock.UtcNow)
        {
            return Result<Response>.Failure(Errors.Share.Expired());
        }

        return Result<Response>.Success(response);
    }
}
