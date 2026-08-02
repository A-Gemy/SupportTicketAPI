using Microsoft.AspNetCore.Mvc;
using SupportTicketAPI.Common;
using SupportTicketAPI.DTOs.Common;

namespace SupportTicketAPI.Extensions
{
    public static class ControllerExtensions
    {
        public static IActionResult ToErrorResponse<TResponse>(
            this ControllerBase controller,
            ResultType resultType,
            string message,
            Dictionary<string, string[]>? errors = null)
        {
            ApiResponse<TResponse> response =
                ApiResponse<TResponse>.Failure(message, errors);

            return resultType switch
            {

                ResultType.ValidationError =>
                    controller.BadRequest(response),

                ResultType.Unauthorized =>
                    controller.Unauthorized(response),

                ResultType.Forbidden =>
                    controller.StatusCode(StatusCodes.Status403Forbidden, response),

                ResultType.NotFound =>
                    controller.NotFound(response),

                ResultType.Conflict =>
                    controller.Conflict(response),

                _ =>
                    controller.StatusCode(StatusCodes.Status500InternalServerError, response)
            };
        }
    }
}
