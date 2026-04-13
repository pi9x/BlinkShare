using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Features.Shares.CreateFileUpload;

public sealed class Validator(IOptions<CreateFileUploadOptions> options) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public Result Validate(Command command)
    {
        if (command.Tier == ShareTier.Anonymous)
        {
            return Result.Failure(Errors.Share.AnonymousRelayNotSupported());
        }

        if (command.Tier != ShareTier.Free)
        {
            return Result.Failure(Errors.Share.UnsupportedTier("Only the free tier is currently handled by this slice."));
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

        if (command.SizeBytes > options.Value.MaxFileSizeBytes)
        {
            return Result.Failure(Errors.Share.FileTooLarge(
                $"File size cannot exceed {options.Value.MaxFileSizeBytes} bytes."));
        }

        return Result.Success();
    }
}
