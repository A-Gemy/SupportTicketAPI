namespace SupportTicketAPI.Common
{
    public static class DatabaseResultCodes
    {
        public const string Success = "SUCCESS";

        public const string EmailAlreadyExists =
            "EMAIL_ALREADY_EXISTS";

        public const string AdminNotFoundOrInactive =
            "ADMIN_NOT_FOUND_OR_INACTIVE";

        public const string UserNotFound =
            "USER_NOT_FOUND";

        public const string OldRefreshTokenHashRequired =
            "OLD_REFRESH_TOKEN_HASH_REQUIRED";

        public const string NewRefreshTokenHashRequired =
            "NEW_REFRESH_TOKEN_HASH_REQUIRED";

        public const string InvalidRefreshTokenExpiration =
            "INVALID_REFRESH_TOKEN_EXPIRATION";

        public const string InvalidRefreshToken =
            "INVALID_REFRESH_TOKEN";

        public const string RefreshTokenRevoked =
            "REFRESH_TOKEN_REVOKED";

        public const string RefreshTokenExpired =
            "REFRESH_TOKEN_EXPIRED";

        public const string AccountInactive =
            "ACCOUNT_INACTIVE";

        public const string RefreshTokenNotFound =
            "REFRESH_TOKEN_NOT_FOUND";

        public const string RefreshTokenAlreadyRevoked =
            "REFRESH_TOKEN_ALREADY_REVOKED";

        public const string RefreshTokenRevocationFailed =
            "REFRESH_TOKEN_REVOCATION_FAILED";
    }
}
