using UniversityApi.DTO.Courses;

namespace UniversityApi.Services.Interfaces;

public interface ICourseService
{
    Task<IEnumerable<CourseDto>> GetAllAsync(string? name, int? teacherId, int? minCredits, int? maxCredits);
    Task<CourseDto> GetByIdAsync(int id);

    // API v2
    Task<IEnumerable<CourseV2Dto>> GetAllV2Async(string? name, int? teacherId, int? minCredits, int? maxCredits);
    Task<CourseV2Dto> GetByIdV2Async(int id);

    Task<CourseDto> CreateAsync(CourseCreateDto dto);
    Task<CourseDto> UpdateAsync(int id, CourseUpdateDto dto);
    Task DeleteAsync(int id);
}
