using UniversityApi.DTO.Teachers;

namespace UniversityApi.Services.Interfaces;

public interface ITeacherService
{
    Task<IEnumerable<TeacherDto>> GetAllAsync();
    Task<TeacherDto> GetByIdAsync(int id);
    Task<TeacherDto> CreateAsync(TeacherCreateDto dto);
    Task<TeacherDto> UpdateAsync(int id, TeacherUpdateDto dto);
    Task DeleteAsync(int id);
}
