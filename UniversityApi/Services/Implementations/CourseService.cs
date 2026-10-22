using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courses;
    private readonly ITeacherRepository _teachers;
    private readonly IMapper _mapper;
    private readonly ILogger<CourseService> _logger;

    public CourseService(ICourseRepository courses, ITeacherRepository teachers,
        IMapper mapper, ILogger<CourseService> logger)
    {
        _courses = courses;
        _teachers = teachers;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CourseDto>> GetAllAsync(string? name, int? teacherId, int? minCredits, int? maxCredits)
    {
        ValidateFilter(minCredits, maxCredits);

        return _mapper.Map<IEnumerable<CourseDto>>(
            await _courses.SearchAsync(name, teacherId, minCredits, maxCredits));
    }

    public async Task<CourseDto> GetByIdAsync(int id)
        => _mapper.Map<CourseDto>(await GetEntityOrThrowAsync(id));

    public async Task<CourseDto> CreateAsync(CourseCreateDto dto)
    {
        await EnsureTeacherExistsAsync(dto.TeacherId);

        var entity = _mapper.Map<Course>(dto);
        await _courses.AddAsync(entity);
        _logger.LogInformation("Course created: Id={CourseId}, Name={Name}, TeacherId={TeacherId}",
            entity.Id, entity.Name, entity.TeacherId);

        return _mapper.Map<CourseDto>(await _courses.GetByIdAsync(entity.Id));
    }

    public async Task<CourseDto> UpdateAsync(int id, CourseUpdateDto dto)
    {
        var entity = await GetEntityOrThrowAsync(id);
        await EnsureTeacherExistsAsync(dto.TeacherId);

        _mapper.Map(dto, entity);
        await _courses.UpdateAsync(entity);
        _logger.LogInformation("Course updated: Id={CourseId}", id);

        return _mapper.Map<CourseDto>(await _courses.GetByIdAsync(id));
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetEntityOrThrowAsync(id);
        await _courses.DeleteAsync(entity);
        _logger.LogInformation("Course deleted: Id={CourseId}", id);
    }

    // ---- API v2: бизнес-логика v1-мен ортақ (валидация, 404 және т.б.), тек DTO басқа ----
    public async Task<IEnumerable<CourseV2Dto>> GetAllV2Async(string? name, int? teacherId, int? minCredits, int? maxCredits)
    {
        ValidateFilter(minCredits, maxCredits);
        return _mapper.Map<IEnumerable<CourseV2Dto>>(
            await _courses.SearchWithDetailsAsync(name, teacherId, minCredits, maxCredits));
    }

    public async Task<CourseV2Dto> GetByIdV2Async(int id)
    {
        var entity = await _courses.GetByIdWithDetailsAsync(id);
        if (entity is null)
        {
            _logger.LogWarning("Course with Id={CourseId} not found", id);
            throw AppException.NotFound("COURSE_NOT_FOUND", "Course not found");
        }
        return _mapper.Map<CourseV2Dto>(entity);
    }

    private static void ValidateFilter(int? minCredits, int? maxCredits)
    {
        if (minCredits.HasValue && maxCredits.HasValue && minCredits > maxCredits)
            throw AppException.BadRequest("INVALID_FILTER", "minCredits cannot be greater than maxCredits");
    }

    private async Task EnsureTeacherExistsAsync(int teacherId)
    {
        if (!await _teachers.ExistsAsync(teacherId))
        {
            _logger.LogWarning("Course refers to non-existent teacher {TeacherId}", teacherId);
            throw AppException.NotFound("TEACHER_NOT_FOUND", "Teacher not found");
        }
    }

    private async Task<Course> GetEntityOrThrowAsync(int id)
    {
        var entity = await _courses.GetByIdAsync(id);
        if (entity is null)
        {
            _logger.LogWarning("Course with Id={CourseId} not found", id);
            throw AppException.NotFound("COURSE_NOT_FOUND", "Course not found");
        }
        return entity;
    }
}
