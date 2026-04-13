using System.Text.RegularExpressions;
using BlinkShare.Api.Common.Results;

namespace BlinkShare.Api.Features.AnonymousSessions.CreateOrJoin;

public sealed partial class Validator : BlinkShare.Api.Common.DependencyInjection.ISliceService
{
    [GeneratedRegex("^[A-Z0-9]{8}$", RegexOptions.CultureInvariant)]
    private static partial Regex CodeRegex();

    public Result Validate(Command command)
    {
        if (string.IsNullOrWhiteSpace(command.Code))
        {
            return Result.Success();
        }

        if (!CodeRegex().IsMatch(command.Code))
        {
            return Result.Failure(Errors.Session.InvalidCode("Session code must be 8 uppercase alphanumeric characters."));
        }

        return Result.Success();
    }
}
