namespace SupportTicketAPI.Common
{
    public enum ResultType
    {
        Success,
        ValidationError,
        Unauthorized,
        Forbidden,
        NotFound,
        Conflict,
        Failure
    }

    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public ResultType ResultType { get; set; }

        public static ServiceResult<T> Success(T data, string message)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message,
                ResultType = ResultType.Success
            };
        }

        public static ServiceResult<T> Failure(
            string message,
            ResultType resultType = ResultType.Failure)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Message = message,
                Data = default,
                ResultType = resultType
            };
        }

        public static ServiceResult<T> ValidationFailure(string message)
        {
            return Failure(message, ResultType.ValidationError);
        }

        public static ServiceResult<T> Unauthorized(string message)
        {
            return Failure(message, ResultType.Unauthorized);
        }

        public static ServiceResult<T> Forbidden(string message)
        {
            return Failure(message, ResultType.Forbidden);
        }

        public static ServiceResult<T> NotFound(string message)
        {
            return Failure(message, ResultType.NotFound);
        }

        public static ServiceResult<T> Conflict(string message)
        {
            return Failure(message, ResultType.Conflict);
        }

    }
}
