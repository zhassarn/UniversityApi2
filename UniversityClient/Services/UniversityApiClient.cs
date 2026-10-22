using System.Text.Json;
using UniversityClient.Models;

namespace UniversityClient.Services;

public class UniversityApiClient : IUniversityApiClient
{
    public const string ClientName = "UniversityApi";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _factory;
    private readonly ILogger<UniversityApiClient> _logger;

    public UniversityApiClient(IHttpClientFactory factory, ILogger<UniversityApiClient> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public Task<ApiResult<List<ClientCourse>>> GetCoursesAsync(string version, CancellationToken ct = default)
        => GetAsync<List<ClientCourse>>($"api/{NormalizeVersion(version)}/courses", ct);

    public Task<ApiResult<ClientCourse>> GetCourseAsync(int id, string version, CancellationToken ct = default)
        => GetAsync<ClientCourse>($"api/{NormalizeVersion(version)}/courses/{id}", ct);

    // URL-ге тек белгілі нұсқалар ғана түседі
    public static string NormalizeVersion(string? version) => version is "v1" or "v2" ? version : "v2";

    private async Task<ApiResult<T>> GetAsync<T>(string relativeUrl, CancellationToken ct)
    {
        try
        {
            var client = _factory.CreateClient(ClientName);
            using var response = await client.GetAsync(relativeUrl, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            ApiEnvelope<T>? envelope = null;
            try { envelope = JsonSerializer.Deserialize<ApiEnvelope<T>>(body, JsonOptions); }
            catch (JsonException) { /* API жарамсыз JSON қайтарды — төменде өңделеді */ }

            if (response.IsSuccessStatusCode)
            {
                if (envelope is { IsSuccess: true, Result: not null })
                    return ApiResult<T>.Ok(envelope.Result);

                _logger.LogError("API returned success status but unexpected body for {Url}", relativeUrl);
                return ApiResult<T>.Fail(502, "API қайтарған жауап форматы дұрыс емес.");
            }

            // Сәтсіз HTTP-код
            _logger.LogWarning("API call {Url} failed with {StatusCode} {ErrorCode}",
                relativeUrl, (int)response.StatusCode, envelope?.ErrorCode);
            return ApiResult<T>.Fail((int)response.StatusCode, MapError((int)response.StatusCode, envelope?.ErrorMessage));
        }
        catch (HttpRequestException ex)
        {
            // API өшірулі, мекенжай қате, желі қатесі
            _logger.LogError(ex, "API is unreachable: {Url}", relativeUrl);
            return ApiResult<T>.Fail(503, "API қолжетімсіз. Сервер қосулы екенін және BaseUrl дұрыс екенін тексеріңіз.");
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "API request timed out: {Url}", relativeUrl);
            return ApiResult<T>.Fail(504, "API уақытында жауап бермеді (timeout).");
        }
    }

    private static string MapError(int statusCode, string? apiMessage) => statusCode switch
    {
        404 => "Сұралған курс табылмады.",
        400 => $"Қате сұраныс: {apiMessage ?? "параметрлерді тексеріңіз"}.",
        401 or 403 => "Бұл ресурсқа қолжеткізу рұқсаты жоқ.",
        >= 500 => "API ішкі қатесі. Кейінірек қайталап көріңіз.",
        _ => apiMessage ?? $"API қатесі ({statusCode})."
    };
}
