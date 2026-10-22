using UniversityApi.DTO.Auth;

namespace UniversityApi.Services.Interfaces;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<UserDto> GetCurrentUserAsync();
}
