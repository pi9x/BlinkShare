using BlinkShare.Api.Common.Results;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Features.AnonymousSessions.PublishFileMetadata;

public sealed class Validator(IOptions<AnonymousSessionOptions> options) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public Result Validate(Command command)
    {
        if (string.IsNullOrWhiteSpace(command.ResumeToken))
        {
            return Result.Failure(Errors.General.Validation("Resume token is required."));
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
