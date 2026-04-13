using BlinkShare.Api.Common.Results;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Features.AnonymousSessions.PublishText;

public sealed class Validator(IOptions<AnonymousSessionOptions> options) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public Result Validate(Command command)
    {
        if (string.IsNullOrWhiteSpace(command.ResumeToken))
        {
            return Result.Failure(Errors.General.Validation("Resume token is required."));
        }

        if (command.Text is null)
        {
            return Result.Failure(Errors.General.Validation("Text is required."));
        }

        if (string.IsNullOrWhiteSpace(command.Text))
        {
            return Result.Failure(Errors.Share.InvalidText("Text cannot be blank."));
        }

        if (command.Text.Length > options.Value.MaxTextLength)
        {
            return Result.Failure(Errors.Share.TextTooLarge(
                $"Text cannot exceed {options.Value.MaxTextLength} characters."));
        }

        return Result.Success();
    }
}
