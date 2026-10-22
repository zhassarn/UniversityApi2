using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Teachers;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _teachers;
    private readonly IMapper _mapper;
    private readonly ILogger<TeacherService> _logger;

    public TeacherService(ITeacherRepository teachers, IMapper mapper, ILogger<TeacherService> logger)
    {
        _teachers = teachers;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<TeacherDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<TeacherDto>>(await _teachers.GetAllAsync());

    public async Task<TeacherDto> GetByIdAsync(int id)
        => _mapper.Map<TeacherDto>(await GetEntityOrThrowAsync(id));

    public async Task<TeacherDto> CreateAsync(TeacherCreateDto dto)
    {
        if (await _teachers.EmailExistsAsync(dto.Email))
        {
            _logger.LogWarning("Teacher creation rejected: email {Email} already exists", dto.Email);
            throw AppException.Conflict("TEACHER_EMAIL_EXISTS", "A teacher with this email already exists");
        }

        var entity = _mapper.Map<Teacher>(dto);
        entity.Email = entity.Email.Trim();
        await _teachers.AddAsync(entity);

        _logger.LogInformation("Teacher created: Id={TeacherId}, Email={Email}", entity.Id, entity.Email);
        return _mapper.Map<TeacherDto>(entity);
    }

    public async Task<TeacherDto> UpdateAsync(int id, TeacherUpdateDto dto)
    {
        var entity = await GetEntityOrThrowAsync(id);

        if (await _teachers.EmailExistsAsync(dto.Email, id))
        {
            _logger.LogWarning("Teacher update rejected: email {Email} already used by another teacher", dto.Email);
            throw AppException.Conflict("TEACHER_EMAIL_EXISTS", "A teacher with this email already exists");
        }

        _mapper.Map(dto, entity);
        entity.Email = entity.Email.Trim();
        await _teachers.UpdateAsync(entity);

        _logger.LogInformation("Teacher updated: Id={TeacherId}", id);
        return _mapper.Map<TeacherDto>(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetEntityOrThrowAsync(id);

        if (await _teachers.HasCoursesAsync(id))
        {
            _logger.LogWarning("Teacher {TeacherId} cannot be deleted: still assigned to courses", id);
            throw AppException.Conflict("TEACHER_HAS_COURSES", "Teacher is assigned to courses and cannot be deleted");
        }

        await _teachers.DeleteAsync(entity);
        _logger.LogInformation("Teacher deleted: Id={TeacherId}", id);
    }

    private async Task<Teacher> GetEntityOrThrowAsync(int id)
    {
        var entity = await _teachers.GetByIdAsync(id);
        if (entity is null)
        {
            _logger.LogWarning("Teacher with Id={TeacherId} not found", id);
            throw AppException.NotFound("TEACHER_NOT_FOUND", "Teacher not found");
        }
        return entity;
    }
}
