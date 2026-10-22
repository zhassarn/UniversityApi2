using UniversityApi.Common;

namespace UniversityApi.Middleware;

/// <summary>
/// Орталықтандырылған қате өңдеу: клиентке StackTrace/SQL/ішкі мәлімет бармайды,
/// толық ақпарат тек логқа жазылады.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            // Белгісіз маршрут (жауап денесі жоқ 404) -> бірыңғай форматқа келтіру
            if (context.Response.StatusCode == StatusCodes.Status404NotFound
                && !context.Response.HasStarted
                && context.Response.ContentLength is null
                && string.IsNullOrEmpty(context.Response.ContentType))
            {
                await ApiResponseWriter.WriteAsync(context, StatusCodes.Status404NotFound,
                    "ROUTE_NOT_FOUND", "Resource not found");
            }
        }
        catch (AppException ex)
        {
            // Күтілетін бизнес-қате: сервис қабатында Warning деңгейінде логталған
            await ApiResponseWriter.WriteAsync(context, ex.StatusCode, ex.ErrorCode, ex.Message, ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception. TraceId={TraceId}, Path={Path}",
                context.TraceIdentifier, context.Request.Path);
            await ApiResponseWriter.WriteAsync(context, StatusCodes.Status500InternalServerError,
                "INTERNAL_SERVER_ERROR", "An unexpected error occurred");
        }
    }
}
