namespace UniversityApi.Common;

public class ReturnResult<T>
{
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public T? Result { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public string? TraceId { get; set; }

    public static ReturnResult<T> Success(T? result, int statusCode, string traceId) => new()
    {
        StatusCode = statusCode,
        IsSuccess = true,
        Result = result,
        TraceId = traceId
    };

    public static ReturnResult<T> Failure(int statusCode, string errorCode, string errorMessage,
        string traceId, Dictionary<string, string[]>? errors = null) => new()
    {
        StatusCode = statusCode,
        IsSuccess = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage,
        Errors = errors,
        TraceId = traceId
    };
}
