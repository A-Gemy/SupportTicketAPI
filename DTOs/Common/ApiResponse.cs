namespace SupportTicketAPI.DTOs.Common
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public Dictionary<string, string[]>? Errors { get; set; }

        public static ApiResponse<T> Success(T? data, string message)
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data,
                Errors = null
            };
        }

        public static ApiResponse<T> Failure(
            string message,
            Dictionary<string, string[]>? errors = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Data = default,
                Errors = errors
            };
        }

    }
}
