using BlinkShare.Api.Common.Auth;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Features.Quotas.GetUsage;

public sealed class Handler(
    ICurrentAccountAccessor currentAccountAccessor,
    IClock clock,
    IOptions<Quotas.QuotaOptions> options) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public Task<Result<Response>> HandleAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currentAccount = currentAccountAccessor.GetCurrentAccount();
        var tier = currentAccount is null ? ShareTier.Anonymous : ShareTier.Free;
        var bytesLimit = tier == ShareTier.Free
            ? options.Value.FreeBytesLimitToday
            : options.Value.AnonymousBytesLimitToday;
        var sharesCreatedLimit = tier == ShareTier.Free
            ? options.Value.FreeSharesCreatedLimitToday
            : options.Value.AnonymousSharesCreatedLimitToday;
        var now = clock.UtcNow;
        var windowEndsAtUtc = new DateTimeOffset(now.UtcDateTime.Date.AddDays(1), TimeSpan.Zero);

        return Task.FromResult(Result<Response>.Success(new Response(
            tier,
            0,
            bytesLimit,
            0,
            windowEndsAtUtc)));
    }
}
