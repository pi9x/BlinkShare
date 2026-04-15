namespace BlinkShare.Api.Common.Results;

public static class Errors
{
    public static class General
    {
        public static Error Validation(string message) =>
            new("general.validation", message);

        public static Error Unexpected(string? message = null) =>
            new("general.unexpected", message ?? "An unexpected error occurred.");
    }

    public static class Share
    {
        public static Error NotFound(string? message = null) =>
            new("share.not_found", message ?? "Share was not found.");

        public static Error Expired(string? message = null) =>
            new("share.expired", message ?? "Share has expired.");

        public static Error PasscodeRequired(string? message = null) =>
            new("share.passcode_required", message ?? "A valid share unlock proof is required.");

        public static Error InvalidPasscode(string? message = null) =>
            new("share.invalid_passcode", message ?? "The provided passcode is invalid.");

        public static Error PasscodeNotRequired(string? message = null) =>
            new("share.passcode_not_required", message ?? "This share does not require a passcode.");

        public static Error InvalidText(string? message = null) =>
            new("share.invalid_text", message ?? "Share text is invalid.");

        public static Error TextTooLarge(string? message = null) =>
            new("share.text_too_large", message ?? "Share text exceeds the maximum allowed length.");

        public static Error InvalidKind(string? message = null) =>
            new("share.invalid_kind", message ?? "The share kind is not valid for this operation.");

        public static Error InvalidStatus(string? message = null) =>
            new("share.invalid_status", message ?? "The share is not in a valid state for this operation.");

        public static Error NotReady(string? message = null) =>
            new("share.not_ready", message ?? "The share is not ready yet.");

        public static Error FileTooLarge(string? message = null) =>
            new("share.file_too_large", message ?? "Share file exceeds the maximum allowed size.");

        public static Error MaxDownloadsReached(string? message = null) =>
            new("share.max_downloads_reached", message ?? "The maximum number of downloads has been reached.");

        public static Error AnonymousRelayNotSupported(string? message = null) =>
            new("share.anonymous_relay_not_supported", message ?? "Anonymous text relay is not handled by this slice.");

        public static Error UnsupportedTier(string? message = null) =>
            new("share.unsupported_tier", message ?? "The requested share tier is not supported by this slice.");

        public static Error CodeUnavailable(string? message = null) =>
            new("share.code_unavailable", message ?? "A unique share code could not be generated.");
    }

    public static class Quota
    {
        public static Error DailyBytesExceeded(string? message = null) =>
            new("quota.daily_bytes_exceeded", message ?? "Daily byte quota has been exceeded.");
    }

    public static class Session
    {
        public static Error NotFound(string? message = null) =>
            new("session.not_found", message ?? "Peer session was not found.");

        public static Error InvalidCode(string? message = null) =>
            new("session.invalid_code", message ?? "The supplied peer session code is invalid.");

        public static Error PeerNotFound(string? message = null) =>
            new("session.peer_not_found", message ?? "Peer was not found in the session.");

        public static Error InvalidResumeToken(string? message = null) =>
            new("session.invalid_resume_token", message ?? "The supplied resume token is invalid.");

        public static Error ReconnectGraceElapsed(string? message = null) =>
            new("session.reconnect_grace_elapsed", message ?? "Reconnect grace has elapsed for this peer.");
    }

    public static class Auth
    {
        public static Error DuplicateEmail(string? message = null) =>
            new("auth.duplicate_email", message ?? "An account with that email already exists.");

        public static Error InvalidCredentials(string? message = null) =>
            new("auth.invalid_credentials", message ?? "Email or password is invalid.");

        public static Error Unauthorized(string? message = null) =>
            new("auth.unauthorized", message ?? "Authentication is required.");
    }
}
