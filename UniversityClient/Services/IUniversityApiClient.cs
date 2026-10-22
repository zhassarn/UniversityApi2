using UniversityClient.Models;

namespace UniversityClient.Services;

public interface IUniversityApiClient
{
    Task<ApiResult<List<ClientCourse>>> GetCoursesAsync(string version, CancellationToken ct = default);
    Task<ApiResult<ClientCourse>> GetCourseAsync(int id, string version, CancellationToken ct = default);
}
