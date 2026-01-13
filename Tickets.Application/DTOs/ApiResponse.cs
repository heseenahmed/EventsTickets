using Microsoft.AspNetCore.Http;
using System.Net;
namespace Tickets.Application.DTOs
{
    public class APIResponse<T>
    {
        public int ApiStatusCode { get; set; }
        public string Result { get; set; }
        public string Msg { get; set; }
        public List<string>? Errors { get; set; }
        public T? Data { get; set; }
        public static APIResponse<T> Success(T data, string message = "Success", int statusCode = StatusCodes.Status200OK)
        {
            return new APIResponse<T>
            {
                ApiStatusCode = statusCode,
                Result = "Success",
                Msg = message,
                Errors = null,
                Data = data
            };
        }
        public static APIResponse<T> Fail(int statusCode, List<string>? errors = null, string? message = "Failed")
        {
            return new APIResponse<T>
            {
                ApiStatusCode = statusCode,
                Result = "Error",
                Msg = message ?? "Failed",
                Errors = errors ?? new List<string> { "An error occurred." },
                Data = default
            };
        }
        public static APIResponse<T> Exception(Exception ex, string? message = "Unexpected error occurred")
        {
            return new APIResponse<T>
            {
                ApiStatusCode = StatusCodes.Status500InternalServerError,
                Result = "Error",
                Msg = message ?? "Exception occurred.",
                Errors = new List<string> { ex.Message },
                Data = default
            };
        }
    }
}
