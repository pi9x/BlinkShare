namespace BlinkShare.Api.Infrastructure.Persistence;

public sealed class Share
{
    private Share()
    {
    }

    public Share(
        Guid id,
        string code,
        ShareTier tier,
        ShareMode mode,
        ShareKind kind,
        ShareStatus status,
        Guid? ownerUserId,
        string? passcodeHash,
        string? textInline,
        string? fileName,
        string? contentType,
        long sizeBytes,
        string? storageKey,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? expiresAtUtc,
        DateTimeOffset? lastAccessedAtUtc,
        int downloadCount,
        int? maxDownloadCount)
    {
        Id = id;
        Code = code;
        Tier = tier;
        Mode = mode;
        Kind = kind;
        Status = status;
        OwnerUserId = ownerUserId;
        PasscodeHash = passcodeHash;
        TextInline = textInline;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        StorageKey = storageKey;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        LastAccessedAtUtc = lastAccessedAtUtc;
        DownloadCount = downloadCount;
        MaxDownloadCount = maxDownloadCount;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public ShareTier Tier { get; private set; }

    public ShareMode Mode { get; private set; }

    public ShareKind Kind { get; private set; }

    public ShareStatus Status { get; private set; }

    public Guid? OwnerUserId { get; private set; }

    public string? PasscodeHash { get; private set; }

    public string? TextInline { get; private set; }

    public string? FileName { get; private set; }

    public string? ContentType { get; private set; }

    public long SizeBytes { get; private set; }

    public string? StorageKey { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? ExpiresAtUtc { get; private set; }

    public DateTimeOffset? LastAccessedAtUtc { get; private set; }

    public int DownloadCount { get; private set; }

    public int? MaxDownloadCount { get; private set; }

    public DateTimeOffset? StorageCleanupCompletedAtUtc { get; private set; }

    public bool HasPasscode() => !string.IsNullOrWhiteSpace(PasscodeHash);

    public void MarkAccessed(DateTimeOffset accessedAtUtc)
    {
        LastAccessedAtUtc = accessedAtUtc;
    }

    public void MarkReady()
    {
        Status = ShareStatus.Ready;
    }

    public void RecordDownload(DateTimeOffset accessedAtUtc)
    {
        DownloadCount++;
        LastAccessedAtUtc = accessedAtUtc;
    }

    public void MarkExpired()
    {
        Status = ShareStatus.Expired;
    }

    public void MarkDeleted()
    {
        Status = ShareStatus.Deleted;
    }

    public void MarkStorageCleanupCompleted(DateTimeOffset completedAtUtc)
    {
        StorageCleanupCompletedAtUtc = completedAtUtc;
    }
}
