using UniversityApi.DTO.Enrollments;

namespace UniversityApi.Services.Interfaces;

public interface IEnrollmentService
{
    Task<IEnumerable<EnrollmentDto>> GetAllAsync();
    Task<EnrollmentDto> GetByIdAsync(int id);
    Task<EnrollmentDto> EnrollAsync(EnrollmentCreateDto dto);
    Task<EnrollmentDto> UpdateGradeAsync(int id, EnrollmentGradeUpdateDto dto);
    Task DeleteAsync(int id);
}
