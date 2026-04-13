using Microsoft.AspNetCore.Http.HttpResults;

namespace BlinkShare.Api.Common.Results;

public static class HttpResultMapping
{
    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess
            ? TypedResults.NoContent()
            : CreateFailureResult(result.Error!);

    public static IResult ToHttpResult<T>(this Result<T> result) =>
        result.ToHttpResult(TypedResults.Ok);

    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);

        if (result.IsFailure)
        {
            return CreateFailureResult(result.Error!);
        }

        return onSuccess(result.Value!);
    }

    private static IResult CreateFailureResult(Error error)
    {
        var statusCode = MapStatusCode(error);

        return TypedResults.Problem(
            statusCode: statusCode,
            title: error.Code,
            detail: error.Message,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = error.Code
            });
    }

    private static int MapStatusCode(Error error) => error.Code switch
    {
        "general.validation" => StatusCodes.Status400BadRequest,
        "share.invalid_text" => StatusCodes.Status400BadRequest,
        "share.text_too_large" => StatusCodes.Status400BadRequest,
        "share.passcode_not_required" => StatusCodes.Status400BadRequest,
        "share.invalid_kind" => StatusCodes.Status400BadRequest,
        "share.file_too_large" => StatusCodes.Status400BadRequest,
        "share.anonymous_relay_not_supported" => StatusCodes.Status400BadRequest,
        "share.unsupported_tier" => StatusCodes.Status400BadRequest,
        "share.passcode_required" => StatusCodes.Status403Forbidden,
        "share.invalid_passcode" => StatusCodes.Status403Forbidden,
        "share.code_unavailable" => StatusCodes.Status500InternalServerError,
        "share.invalid_status" => StatusCodes.Status409Conflict,
        "share.not_ready" => StatusCodes.Status409Conflict,
        "share.max_downloads_reached" => StatusCodes.Status409Conflict,
        "share.not_found" => StatusCodes.Status404NotFound,
        "share.expired" => StatusCodes.Status410Gone,
        "session.invalid_code" => StatusCodes.Status400BadRequest,
        "session.invalid_resume_token" => StatusCodes.Status403Forbidden,
        "session.peer_not_found" => StatusCodes.Status404NotFound,
        "session.not_found" => StatusCodes.Status404NotFound,
        "session.expired" => StatusCodes.Status410Gone,
        "session.reconnect_grace_elapsed" => StatusCodes.Status410Gone,
        "auth.duplicate_email" => StatusCodes.Status409Conflict,
        "auth.invalid_credentials" => StatusCodes.Status401Unauthorized,
        "auth.unauthorized" => StatusCodes.Status401Unauthorized,
        "quota.daily_bytes_exceeded" => StatusCodes.Status429TooManyRequests,
        "general.unexpected" => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status400BadRequest
    };
}
