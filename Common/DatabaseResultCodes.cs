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

        public const string CustomerNotFoundOrInactive =
            "CUSTOMER_NOT_FOUND_OR_INACTIVE";

        public const string InvalidPriority =
            "INVALID_PRIORITY";

        public const string InvalidPageNumber =
            "INVALID_PAGE_NUMBER";

        public const string InvalidPageSize =
            "INVALID_PAGE_SIZE";

        public const string TicketNotFound =
            "TICKET_NOT_FOUND";

        public const string TicketAlreadyClosed =
            "TICKET_ALREADY_CLOSED";

        public const string TicketCloseFailed =
            "TICKET_CLOSE_FAILED";

        public const string ActionRequired =
            "ACTION_REQUIRED";

        public const string InvalidDateRange =
            "INVALID_DATE_RANGE";

        public const string AgentNotFoundOrInactive =
            "AGENT_NOT_FOUND_OR_INACTIVE";

        public const string ClosedTicketCannotBeAssigned =
            "CLOSED_TICKET_CANNOT_BE_ASSIGNED";

        public const string TicketAlreadyAssignedToAgent =
            "TICKET_ALREADY_ASSIGNED_TO_AGENT";

        public const string InvalidTicketStatus =
            "INVALID_TICKET_STATUS";

        public const string ClosedTicketCannotBeUpdated =
            "CLOSED_TICKET_CANNOT_BE_UPDATED";

        public const string AssignedTicketCannotBeMovedToOpen =
            "ASSIGNED_TICKET_CANNOT_BE_MOVED_TO_OPEN";

        public const string TicketMustBeAssignedForStatus =
            "TICKET_MUST_BE_ASSIGNED_FOR_STATUS";

        public const string TicketAlreadyHasRequestedStatus =
            "TICKET_ALREADY_HAS_REQUESTED_STATUS";

        public const string ResolvedTicketCannotBeUpdatedByAgent =
            "RESOLVED_TICKET_CANNOT_BE_UPDATED_BY_AGENT";

        public const string OnlyAssignedTicketCanMoveToInProgress =
            "ONLY_ASSIGNED_TICKET_CAN_MOVE_TO_IN_PROGRESS";

        public const string TicketMustBeInProgressBeforeResolved =
            "TICKET_MUST_BE_IN_PROGRESS_BEFORE_RESOLVED";

        public const string CommentTextRequired =
            "COMMENT_TEXT_REQUIRED";

        public const string UserNotFoundOrInactive =
            "USER_NOT_FOUND_OR_INACTIVE";

        public const string ClosedTicketCannotBeCommented =
            "CLOSED_TICKET_CANNOT_BE_COMMENTED";

        public const string TicketCommentAddForbidden =
            "TICKET_COMMENT_ADD_FORBIDDEN";

        public const string TicketCommentReadForbidden =
            "TICKET_COMMENT_READ_FORBIDDEN";

    }
}
