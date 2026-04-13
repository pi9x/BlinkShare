using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.ObjectStorage;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Features.Shares.CreateFileUpload;

public sealed class Handler(
    IDbContextFactory<BlinkShareDbContext> dbContextFactory,
    Validator validator,
    ICodeGenerator codeGenerator,
    IObjectStorage objectStorage,
    IClock clock,
    IOptions<CreateFileUploadOptions> options) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    private const int MaxCodeGenerationAttempts = 5;

    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(command);
        if (validationResult.IsFailure)
        {
            return Result<Response>.Failure(validationResult.Error!);
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var codeResult = await GenerateUniqueCodeAsync(dbContext, cancellationToken);
        if (codeResult.IsFailure)
        {
            return Result<Response>.Failure(codeResult.Error!);
        }

        var createdAtUtc = clock.UtcNow;
        var expiresAtUtc = createdAtUtc.AddMinutes(options.Value.FreeTierTtlMinutes);
        var code = codeResult.Value!;
        var storageKey = CreateStorageKey(code, command.FileName!);

        var share = new Share(
            id: Guid.NewGuid(),
            code: code,
            tier: command.Tier,
            mode: ShareMode.StoredShare,
            kind: ShareKind.File,
            status: ShareStatus.Pending,
            ownerUserId: null,
            passcodeHash: null,
            textInline: null,
            fileName: command.FileName,
            contentType: command.ContentType,
            sizeBytes: command.SizeBytes,
            storageKey: storageKey,
            createdAtUtc: createdAtUtc,
            expiresAtUtc: expiresAtUtc,
            lastAccessedAtUtc: null,
            downloadCount: 0,
            maxDownloadCount: null);

        var uploadTarget = await objectStorage.CreateUploadTargetAsync(
            new ObjectStorageUploadRequest(storageKey, command.ContentType!, command.SizeBytes, expiresAtUtc),
            cancellationToken);

        await dbContext.Shares.AddAsync(share, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<Response>.Success(new Response(
            share.Id,
            share.Code,
            storageKey,
            uploadTarget.UploadUrl,
            expiresAtUtc));
    }

    private async Task<Result<string>> GenerateUniqueCodeAsync(
        BlinkShareDbContext dbContext,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxCodeGenerationAttempts; attempt++)
        {
            var code = codeGenerator.GenerateShareCode();
            var exists = await dbContext.Shares
                .AsNoTracking()
                .AnyAsync(share => share.Code == code, cancellationToken);

            if (!exists)
            {
                return Result<string>.Success(code);
            }
        }

        return Result<string>.Failure(Errors.Share.CodeUnavailable());
    }

    private static string CreateStorageKey(string code, string fileName) =>
        $"shares/{code}/{Guid.NewGuid():N}/{Path.GetFileName(fileName)}";
}
