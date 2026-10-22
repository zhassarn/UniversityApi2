using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Enrollments;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollments;
    private readonly IStudentRepository _students;
    private readonly ICourseRepository _courses;
    private readonly ICurrentUserService _user;
    private readonly IMapper _mapper;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(IEnrollmentRepository enrollments, IStudentRepository students,
        ICourseRepository courses, ICurrentUserService user, IMapper mapper, ILogger<EnrollmentService> logger)
    {
        _enrollments = enrollments;
        _students = students;
        _courses = courses;
        _user = user;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<EnrollmentDto>> GetAllAsync()
    {
        // Рөлге қарай көрінетін деректер: Admin — бәрі, Teacher — өз курстары, Student — өз жазылулары
        IReadOnlyList<Enrollment> list;
        if (_user.IsAdmin)
            list = await _enrollments.GetAllAsync();
        else if (_user.IsTeacher)
            list = _user.TeacherId is int teacherId
                ? await _enrollments.GetFilteredAsync(null, teacherId)
                : Array.Empty<Enrollment>();
        else if (_user.IsStudent)
            list = _user.StudentId is int studentId
                ? await _enrollments.GetFilteredAsync(studentId, null)
                : Array.Empty<Enrollment>();
        else
            list = Array.Empty<Enrollment>();

        return _mapper.Map<IEnumerable<EnrollmentDto>>(list);
    }

    public async Task<EnrollmentDto> GetByIdAsync(int id)
    {
        var entity = await GetEntityOrThrowAsync(id);

        var allowed = _user.IsAdmin
            || (_user.IsTeacher && _user.TeacherId.HasValue && entity.Course.TeacherId == _user.TeacherId.Value)
            || (_user.IsStudent && _user.StudentId.HasValue && entity.StudentId == _user.StudentId.Value);
        if (!allowed) throw Denied($"read enrollment {id}");

        return _mapper.Map<EnrollmentDto>(entity);
    }

    public async Task<EnrollmentDto> EnrollAsync(EnrollmentCreateDto dto)
    {
        // Ownership: Student тек өзін жаза алады, Admin — кез келген студентті
        if (!_user.IsAdmin && !(_user.IsStudent && _user.StudentId == dto.StudentId))
            throw Denied($"enroll student {dto.StudentId}");

        // Ереже №2: жоқ студентті жазуға болмайды
        if (!await _students.ExistsAsync(dto.StudentId))
        {
            _logger.LogWarning("Enrollment rejected: student {StudentId} not found", dto.StudentId);
            throw AppException.NotFound("STUDENT_NOT_FOUND", "Student not found");
        }

        // Ереже №1: жоқ курсқа жазуға болмайды
        if (!await _courses.ExistsAsync(dto.CourseId))
        {
            _logger.LogWarning("Enrollment rejected: course {CourseId} not found", dto.CourseId);
            throw AppException.NotFound("COURSE_NOT_FOUND", "Course not found");
        }

        // Ереже №3: қайта жазуға болмайды
        if (await _enrollments.IsEnrolledAsync(dto.StudentId, dto.CourseId))
        {
            _logger.LogWarning("Enrollment rejected: student {StudentId} already enrolled in course {CourseId}",
                dto.StudentId, dto.CourseId);
            throw AppException.Conflict("ALREADY_ENROLLED", "Student is already enrolled in this course");
        }

        var entity = new Enrollment
        {
            StudentId = dto.StudentId,
            CourseId = dto.CourseId,
            EnrollmentDate = DateTime.UtcNow
        };
        await _enrollments.AddAsync(entity);
        _logger.LogInformation("Enrollment created: Id={EnrollmentId}, StudentId={StudentId}, CourseId={CourseId}",
            entity.Id, entity.StudentId, entity.CourseId);

        return _mapper.Map<EnrollmentDto>(await _enrollments.GetByIdAsync(entity.Id));
    }

    public async Task<EnrollmentDto> UpdateGradeAsync(int id, EnrollmentGradeUpdateDto dto)
    {
        var entity = await GetEntityOrThrowAsync(id);

        // Ownership: Teacher тек өз курсындағы бағаны өзгерте алады (рөл тексеруі жеткіліксіз)
        var allowed = _user.IsAdmin
            || (_user.IsTeacher && _user.TeacherId.HasValue && entity.Course.TeacherId == _user.TeacherId.Value);
        if (!allowed) throw Denied($"change grade of enrollment {id}");

        entity.Grade = dto.Grade;
        await _enrollments.UpdateAsync(entity);

        _logger.LogInformation("Enrollment {EnrollmentId} grade updated to {Grade} by user {UserId}",
            id, dto.Grade, _user.UserId);
        return _mapper.Map<EnrollmentDto>(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetEntityOrThrowAsync(id);
        await _enrollments.DeleteAsync(entity);
        _logger.LogInformation("Enrollment deleted: Id={EnrollmentId}", id);
    }

    private async Task<Enrollment> GetEntityOrThrowAsync(int id)
    {
        var entity = await _enrollments.GetByIdAsync(id);
        if (entity is null)
        {
            _logger.LogWarning("Enrollment with Id={EnrollmentId} not found", id);
            throw AppException.NotFound("ENROLLMENT_NOT_FOUND", "Enrollment not found");
        }
        return entity;
    }

    private AppException Denied(string action)
    {
        _logger.LogWarning("Access denied: user {UserId} tried to {Action}", _user.UserId, action);
        return AppException.Forbidden();
    }
}
