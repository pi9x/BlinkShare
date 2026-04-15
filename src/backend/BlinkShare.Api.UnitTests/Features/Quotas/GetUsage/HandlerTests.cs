using BlinkShare.Api.Common.Auth;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Features.Quotas;
using BlinkShare.Api.Features.Quotas.GetUsage;
using BlinkShare.Api.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.UnitTests.Features.Quotas.GetUsage;

public sealed class HandlerTests
{
    [Fact]
    public async Task Returns_free_tier_usage_for_authenticated_account()
    {
        var handler = new Handler(
            new FakeCurrentAccountAccessor(new CurrentAccount(Guid.NewGuid(), "demo@example.com", AccountTier.Free)),
            new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)),
            Options.Create(new QuotaOptions
            {
                AnonymousBytesLimitToday = 1,
                FreeBytesLimitToday = 456,
                AnonymousSharesCreatedLimitToday = 2,
                FreeSharesCreatedLimitToday = 7
            }));

        var result = await handler.HandleAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AccountTier.Free, result.Value!.Tier);
        Assert.Equal(456, result.Value.BytesLimitToday);
        Assert.Equal(new DateTimeOffset(2026, 4, 13, 0, 0, 0, TimeSpan.Zero), result.Value.WindowEndsAtUtc);
    }

    [Fact]
    public async Task Returns_anonymous_tier_usage_without_authenticated_account()
    {
        var handler = new Handler(
            new FakeCurrentAccountAccessor(null),
            new FakeClock(new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero)),
            Options.Create(new QuotaOptions
            {
                AnonymousBytesLimitToday = 123,
                FreeBytesLimitToday = 456,
                AnonymousSharesCreatedLimitToday = 2,
                FreeSharesCreatedLimitToday = 7
            }));

        var result = await handler.HandleAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AccountTier.Anonymous, result.Value!.Tier);
        Assert.Equal(123, result.Value.BytesLimitToday);
    }

    private sealed class FakeCurrentAccountAccessor(CurrentAccount? currentAccount) : ICurrentAccountAccessor
    {
        public CurrentAccount? GetCurrentAccount() => currentAccount;
    }

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
