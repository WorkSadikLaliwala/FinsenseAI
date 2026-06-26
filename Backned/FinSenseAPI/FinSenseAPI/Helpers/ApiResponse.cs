using System.Text.Json.Serialization;

namespace FinSenseAPI.Helpers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public object Error { get; set; }
    public int StatusCode { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "") => new ApiResponse<T>
    {
        Success = true,
        Message = message,
        Data = data,
        Error = null,
        StatusCode = 200
    };

    public static ApiResponse<T> Fail(object error, int code = 400) => new ApiResponse<T>
    {
        Success = false,
        Message = string.Empty,
        Data = default,
        Error = error,
        StatusCode = code
    };
}
