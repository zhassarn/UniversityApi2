using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface ICourseRepository : IRepository<Course>
{
    Task<IReadOnlyList<Course>> SearchAsync(string? name, int? teacherId, int? minCredits, int? maxCredits);

    // v2: оқытушы + жазылулар (санау үшін)
    Task<IReadOnlyList<Course>> SearchWithDetailsAsync(string? name, int? teacherId, int? minCredits, int? maxCredits);
    Task<Course?> GetByIdWithDetailsAsync(int id);
}
