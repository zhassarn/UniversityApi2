using UniversityApi.DTO.Auth;

namespace UniversityApi.Services.Interfaces;

public interface IUserAdminService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto> SetRoleAsync(string userId, UserRoleDto dto);
    Task<UserDto> LinkAsync(string userId, UserLinkDto dto);
}
