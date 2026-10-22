using UniversityApi.DTO.Students;

namespace UniversityApi.Services.Interfaces;

public interface IStudentService
{
    Task<IEnumerable<StudentDto>> GetAllAsync();
    Task<StudentDto> GetByIdAsync(int id);
    Task<StudentWithCoursesDto> GetWithCoursesAsync(int id);
    Task<StudentDto> CreateAsync(StudentCreateDto dto);
    Task<StudentDto> UpdateAsync(int id, StudentUpdateDto dto);
    Task DeleteAsync(int id);
}
