using BlinkShare.Api.Common.Results;

namespace BlinkShare.Api.Features.Auth.Register;

public sealed class Validator : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    public Result Validate(Command command)
    {
        if (command.Email is null)
        {
            return Result.Failure(Errors.General.Validation("Email is required."));
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return Result.Failure(Errors.General.Validation("Email cannot be blank."));
        }

        if (command.Password is null)
        {
            return Result.Failure(Errors.General.Validation("Password is required."));
        }

        return Result.Success();
    }
}
