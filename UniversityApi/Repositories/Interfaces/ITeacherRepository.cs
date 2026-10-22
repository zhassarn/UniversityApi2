using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface ITeacherRepository : IRepository<Teacher>
{
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    Task<bool> HasCoursesAsync(int teacherId);
}
