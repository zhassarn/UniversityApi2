using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Students;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _students;
    private readonly ICurrentUserService _user;
    private readonly IMapper _mapper;
    private readonly ILogger<StudentService> _logger;

    public StudentService(IStudentRepository students, ICurrentUserService user, IMapper mapper,
        ILogger<StudentService> logger)
    {
        _students = students;
        _user = user;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<StudentDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<StudentDto>>(await _students.GetAllAsync());

    public async Task<StudentDto> GetByIdAsync(int id)
    {
        EnsureAdminOrSelf(id);
        return _mapper.Map<StudentDto>(await GetEntityOrThrowAsync(id));
    }

    public async Task<StudentWithCoursesDto> GetWithCoursesAsync(int id)
    {
        EnsureAdminOrSelf(id);
        var student = await _students.GetWithCoursesAsync(id);
        if (student is null) throw NotFound(id);
        return _mapper.Map<StudentWithCoursesDto>(student);
    }

    public async Task<StudentDto> CreateAsync(StudentCreateDto dto)
    {
        if (await _students.EmailExistsAsync(dto.Email))
        {
            _logger.LogWarning("Student creation rejected: email {Email} already exists", dto.Email);
            throw AppException.Conflict("STUDENT_EMAIL_EXISTS", "A student with this email already exists");
        }

        var entity = _mapper.Map<Student>(dto);
        entity.Email = entity.Email.Trim();
        await _students.AddAsync(entity);

        _logger.LogInformation("Student created: Id={StudentId}, Email={Email}", entity.Id, entity.Email);
        return _mapper.Map<StudentDto>(entity);
    }

    public async Task<StudentDto> UpdateAsync(int id, StudentUpdateDto dto)
    {
        var entity = await GetEntityOrThrowAsync(id);

        if (await _students.EmailExistsAsync(dto.Email, id))
        {
            _logger.LogWarning("Student update rejected: email {Email} already used by another student", dto.Email);
            throw AppException.Conflict("STUDENT_EMAIL_EXISTS", "A student with this email already exists");
        }

        _mapper.Map(dto, entity);
        entity.Email = entity.Email.Trim();
        await _students.UpdateAsync(entity);

        _logger.LogInformation("Student updated: Id={StudentId}", id);
        return _mapper.Map<StudentDto>(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetEntityOrThrowAsync(id);
        await _students.DeleteAsync(entity);
        _logger.LogInformation("Student deleted: Id={StudentId}", id);
    }

    private async Task<Student> GetEntityOrThrowAsync(int id)
        => await _students.GetByIdAsync(id) ?? throw NotFound(id);

    // Ownership: Student тек өз деректерін (және бағаларын) көре алады, Admin — кез келгенін
    private void EnsureAdminOrSelf(int studentId)
    {
        if (_user.IsAdmin) return;
        if (_user.IsStudent && _user.StudentId == studentId) return;

        _logger.LogWarning("Access denied: user {UserId} tried to read student {StudentId}", _user.UserId, studentId);
        throw AppException.Forbidden();
    }

    private AppException NotFound(int id)
    {
        _logger.LogWarning("Student with Id={StudentId} not found", id);
        return AppException.NotFound("STUDENT_NOT_FOUND", "Student not found");
    }
}
