namespace UniversityApi.Common;

/// <summary>Бизнес-ережелер бұзылғанда лақтырылатын күтілетін қате.</summary>
public class AppException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public Dictionary<string, string[]>? Errors { get; }

    public AppException(int statusCode, string errorCode, string message,
        Dictionary<string, string[]>? errors = null) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Errors = errors;
    }

    public static AppException NotFound(string errorCode, string message) => new(StatusCodes.Status404NotFound, errorCode, message);
    public static AppException Conflict(string errorCode, string message) => new(StatusCodes.Status409Conflict, errorCode, message);
    public static AppException BadRequest(string errorCode, string message, Dictionary<string, string[]>? errors = null)
        => new(StatusCodes.Status400BadRequest, errorCode, message, errors);
    public static AppException Unauthorized(string errorCode, string message) => new(StatusCodes.Status401Unauthorized, errorCode, message);
    public static AppException Forbidden(string message = "You do not have permission to perform this action")
        => new(StatusCodes.Status403Forbidden, "FORBIDDEN", message);
}
