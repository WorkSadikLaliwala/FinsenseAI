using FinSenseAPI.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class GlobalApiResponseFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        // Intercept ObjectResults (responses returning DTOs/data)
        if (context.Result is ObjectResult objectResult && objectResult.Value is not null)
        {
            var valueType = objectResult.Value.GetType();

            // Check if it's already an ApiResponse to avoid double-wrapping
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(ApiResponse<>))
            {
                return;
            }

            // Wrap the returned DTO in the ApiResponse envelope
            var wrappedResult = new ApiResponse<object>
            {
                Success = true,
                Message = "Success",
                Data = objectResult.Value,
                Error = null,
                StatusCode = objectResult.StatusCode ?? 200
            };

            // Replace the result value
            objectResult.Value = wrappedResult;
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
