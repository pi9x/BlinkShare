using System.Text;
using BlinkShare.Api.Common.Auth;
using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Common.Time;
using BlinkShare.Api.Infrastructure.Persistence;
using BlinkShare.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Features.Shares.CreateText;

public sealed class Handler(
    IDbContextFactory<BlinkShareDbContext> dbContextFactory,
    Validator validator,
    ICodeGenerator codeGenerator,
    ICurrentAccountAccessor currentAccountAccessor,
    IClock clock,
    IOptions<CreateTextOptions> options) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    private const int MaxCodeGenerationAttempts = 5;

    public async Task<Result<Response>> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        var effectiveCommand = command with
        {
            Tier = currentAccountAccessor.GetCurrentAccount()?.Tier ?? AccountTier.Anonymous
        };

        var validationResult = validator.Validate(effectiveCommand);
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
        var text = effectiveCommand.Text!;

        var share = new Share(
            id: Guid.NewGuid(),
            code: codeResult.Value!,
            mode: ShareMode.StoredShare,
            kind: ShareKind.Text,
            status: ShareStatus.Ready,
            ownerUserId: null,
            passcodeHash: null,
            textInline: text,
            fileName: null,
            contentType: "text/plain; charset=utf-8",
            sizeBytes: Encoding.UTF8.GetByteCount(text),
            storageKey: null,
            createdAtUtc: createdAtUtc,
            expiresAtUtc: expiresAtUtc,
            lastAccessedAtUtc: null,
            downloadCount: 0,
            maxDownloadCount: null);

        await dbContext.Shares.AddAsync(share, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<Response>.Success(new Response(share.Id, share.Code, expiresAtUtc));
    }

    private async Task<Result<string>> GenerateUniqueCodeAsync(
        BlinkShareDbContext dbContext,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxCodeGenerationAttempts; attempt++)
        {
            var code = codeGenerator.GenerateShareCode();
            var exists = await dbContext.Shares
                .AnyAsync(share => share.Code == code, cancellationToken);

            if (!exists)
            {
                return Result<string>.Success(code);
            }
        }

        return Result<string>.Failure(Errors.Share.CodeUnavailable());
    }
}
