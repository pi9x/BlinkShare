using BlinkShare.Api.Common.Results;
using BlinkShare.Api.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace BlinkShare.Api.Features.Shares.CreateText;

public sealed class Validator(IOptions<CreateTextOptions> options) : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public Result Validate(Command command)
    {
        if (command.Tier == AccountTier.Anonymous)
        {
            return Result.Failure(Errors.Share.AnonymousRelayNotSupported());
        }

        if (command.Tier != AccountTier.Free)
        {
            return Result.Failure(Errors.Share.UnsupportedTier("Only the free tier is currently handled by this slice."));
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
