namespace BlinkShare.Api.Features.Quotas;

public sealed class QuotaOptions
{
    public long AnonymousBytesLimitToday { get; set; } = 0;

    public long FreeBytesLimitToday { get; set; } = 10 * 1024 * 1024;

    public int AnonymousSharesCreatedLimitToday { get; set; } = 0;

    public int FreeSharesCreatedLimitToday { get; set; } = 100;
}
