using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Features.Shares.CreateFileUpload;

public sealed class Validator(IOptions<CreateFileUploadOptions> options) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public Result Validate(Command command)
    {
        if (command.Tier is not AccountTier.Anonymous and not AccountTier.Free)
        {
            return Result.Failure(Errors.Share.UnsupportedTier("Only anonymous and free accounts are currently supported."));
        }

        if (string.IsNullOrWhiteSpace(command.FileName))
        {
            return Result.Failure(Errors.General.Validation("File name is required."));
        }

        if (string.IsNullOrWhiteSpace(command.ContentType))
        {
            return Result.Failure(Errors.General.Validation("Content type is required."));
        }

        if (command.SizeBytes <= 0)
        {
            return Result.Failure(Errors.General.Validation("File size must be greater than zero."));
        }

        var maxFileSizeBytes = command.Tier == AccountTier.Free
            ? options.Value.FreeMaxFileSizeBytes
            : options.Value.AnonymousMaxFileSizeBytes;

        if (command.SizeBytes > maxFileSizeBytes)
        {
            return Result.Failure(Errors.Share.FileTooLarge(
                $"File size cannot exceed {maxFileSizeBytes} bytes."));
        }

        return Result.Success();
    }
}
