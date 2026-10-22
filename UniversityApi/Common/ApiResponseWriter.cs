using System.Text.Json;

namespace UniversityApi.Common;

public static class ApiResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task WriteAsync(HttpContext context, int statusCode, string errorCode, string message,
        Dictionary<string, string[]>? errors = null)
    {
        if (context.Response.HasStarted) return;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        if (statusCode == StatusCodes.Status401Unauthorized)
            context.Response.Headers["WWW-Authenticate"] = "Bearer";

        var body = ReturnResult<object>.Failure(statusCode, errorCode, message, context.TraceIdentifier, errors);
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }
}
