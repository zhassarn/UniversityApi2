using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Common;
using UniversityApi.DTO.Auth;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class UserAdminService : IUserAdminService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IStudentRepository _students;
    private readonly ITeacherRepository _teachers;
    private readonly ICurrentUserService _current;
    private readonly IMapper _mapper;
    private readonly ILogger<UserAdminService> _logger;

    public UserAdminService(UserManager<ApplicationUser> users, IStudentRepository students,
        ITeacherRepository teachers, ICurrentUserService current, IMapper mapper,
        ILogger<UserAdminService> logger)
    {
        _users = users;
        _students = students;
        _teachers = teachers;
        _current = current;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var list = await _users.Users.AsNoTracking().OrderBy(u => u.UserName).ToListAsync();
        var result = new List<UserDto>();
        foreach (var user in list) result.Add(await ToDtoAsync(user));
        return result;
    }

    public async Task<UserDto> SetRoleAsync(string userId, UserRoleDto dto)
    {
        var role = AppRoles.All.FirstOrDefault(r => string.Equals(r, dto.Role, StringComparison.OrdinalIgnoreCase));
        if (role is null)
            throw AppException.BadRequest("INVALID_ROLE", $"Role must be one of: {string.Join(", ", AppRoles.All)}");

        var user = await GetUserOrThrowAsync(userId);

        if (user.Id == _current.UserId)
        {
            _logger.LogWarning("Admin {UserId} tried to change own role", user.Id);
            throw AppException.BadRequest("CANNOT_CHANGE_OWN_ROLE", "You cannot change your own role");
        }

        var currentRoles = await _users.GetRolesAsync(user);
        if (currentRoles.Count > 0)
            ThrowIfFailed(await _users.RemoveFromRolesAsync(user, currentRoles));
        ThrowIfFailed(await _users.AddToRoleAsync(user, role));

        _logger.LogInformation("Role of user {TargetUserId} set to {Role} by admin {AdminId}",
            user.Id, role, _current.UserId);
        return await ToDtoAsync(user);
    }

    public async Task<UserDto> LinkAsync(string userId, UserLinkDto dto)
    {
        var user = await GetUserOrThrowAsync(userId);

        if (dto.StudentId.HasValue)
        {
            if (!await _students.ExistsAsync(dto.StudentId.Value))
                throw AppException.NotFound("STUDENT_NOT_FOUND", "Student not found");
            if (await _users.Users.AnyAsync(u => u.StudentId == dto.StudentId && u.Id != user.Id))
                throw AppException.Conflict("ALREADY_LINKED", "This student is already linked to another account");
        }
        if (dto.TeacherId.HasValue)
        {
            if (!await _teachers.ExistsAsync(dto.TeacherId.Value))
                throw AppException.NotFound("TEACHER_NOT_FOUND", "Teacher not found");
            if (await _users.Users.AnyAsync(u => u.TeacherId == dto.TeacherId && u.Id != user.Id))
                throw AppException.Conflict("ALREADY_LINKED", "This teacher is already linked to another account");
        }

        user.StudentId = dto.StudentId;
        user.TeacherId = dto.TeacherId;
        ThrowIfFailed(await _users.UpdateAsync(user));

        _logger.LogInformation("User {TargetUserId} linked: StudentId={StudentId}, TeacherId={TeacherId} (by admin {AdminId})",
            user.Id, dto.StudentId, dto.TeacherId, _current.UserId);
        return await ToDtoAsync(user);
    }

    private async Task<ApplicationUser> GetUserOrThrowAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("User {TargetUserId} not found", userId);
            throw AppException.NotFound("USER_NOT_FOUND", "User not found");
        }
        return user;
    }

    private async Task<UserDto> ToDtoAsync(ApplicationUser user)
    {
        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = (await _users.GetRolesAsync(user)).ToList();
        return dto;
    }

    private static void ThrowIfFailed(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException("Identity operation failed: " +
                string.Join("; ", result.Errors.Select(e => e.Code)));
    }
}
