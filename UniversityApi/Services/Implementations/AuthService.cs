using AutoMapper;
using Microsoft.AspNetCore.Identity;
using UniversityApi.Common;
using UniversityApi.DTO.Auth;
using UniversityApi.Models;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly ITokenService _tokens;
    private readonly ICurrentUserService _current;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn,
        ITokenService tokens, ICurrentUserService current, IMapper mapper, ILogger<AuthService> logger)
    {
        _users = users;
        _signIn = signIn;
        _tokens = tokens;
        _current = current;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.UserName.Trim(),
            Email = dto.Email.Trim()
        };

        // Пароль хэштелуі Identity (PasswordHasher) арқылы жүреді
        var result = await _users.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Registration rejected for user name {UserName}: {Codes}",
                dto.UserName, string.Join(",", result.Errors.Select(e => e.Code)));
            throw ToException(result);
        }

        // Тіркелген пайдаланушы тек ең төменгі рөл (Student) алады. Admin/Teacher — тек Admin тағайындайды.
        await _users.AddToRoleAsync(user, AppRoles.Student);

        _logger.LogInformation("User registered: Id={UserId}, UserName={UserName}", user.Id, user.UserName);
        return await ToDtoAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _users.FindByNameAsync(dto.UserName.Trim());
        if (user is null)
        {
            _logger.LogWarning("Failed login: unknown user name {UserName}", dto.UserName);
            throw AppException.Unauthorized("INVALID_CREDENTIALS", "Invalid user name or password");
        }

        // lockoutOnFailure: true -> бірнеше сәтсіз әрекеттен кейін аккаунт бұғатталады
        var result = await _signIn.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            _logger.LogWarning("Login blocked: account {UserId} is locked out", user.Id);
            throw AppException.Unauthorized("ACCOUNT_LOCKED", "Account is temporarily locked. Try again later");
        }
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed login for user {UserId}", user.Id);
            throw AppException.Unauthorized("INVALID_CREDENTIALS", "Invalid user name or password");
        }

        var roles = await _users.GetRolesAsync(user);
        var (token, expires) = _tokens.CreateAccessToken(user, roles);

        _logger.LogInformation("User logged in: Id={UserId}", user.Id);
        return new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAtUtc = expires,
            User = await ToDtoAsync(user)
        };
    }

    public async Task<UserDto> GetCurrentUserAsync()
    {
        var id = _current.UserId;
        var user = id is null ? null : await _users.FindByIdAsync(id);
        if (user is null)
            throw AppException.Unauthorized("UNAUTHORIZED", "User no longer exists");
        return await ToDtoAsync(user);
    }

    private async Task<UserDto> ToDtoAsync(ApplicationUser user)
    {
        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = (await _users.GetRolesAsync(user)).ToList();
        return dto;
    }

    private static AppException ToException(IdentityResult result)
    {
        if (result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.DuplicateUserName)))
            return AppException.Conflict("USERNAME_TAKEN", "This user name is already taken");
        if (result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.DuplicateEmail)))
            return AppException.Conflict("EMAIL_TAKEN", "A user with this email already exists");

        var errors = result.Errors
            .GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
        return AppException.BadRequest("VALIDATION_ERROR", "Registration failed", errors);
    }
}
