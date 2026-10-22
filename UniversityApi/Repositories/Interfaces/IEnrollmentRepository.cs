using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface IEnrollmentRepository : IRepository<Enrollment>
{
    Task<bool> IsEnrolledAsync(int studentId, int courseId);
    Task<IReadOnlyList<Enrollment>> GetFilteredAsync(int? studentId, int? teacherId);
}
