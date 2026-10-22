using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "UserName is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "UserName must be 3-50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "UserName may contain only letters, digits, '.', '_' and '-'")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email has invalid format")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;
    // Рөл өрісі әдейі жоқ: тіркелген пайдаланушы өзіне рөл тағайындай алмайды.
}

public class LoginDto
{
    [Required(ErrorMessage = "UserName is required")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Пайдаланушы туралы қауіпсіз ақпарат (парольдік хэш, security stamp және т.б. ЖОҚ).</summary>
public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = new();
    public int? StudentId { get; set; }
    public int? TeacherId { get; set; }
}

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAtUtc { get; set; }
    public UserDto User { get; set; } = new();
}

public class UserRoleDto
{
    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = string.Empty;
}

public class UserLinkDto
{
    public int? StudentId { get; set; }
    public int? TeacherId { get; set; }
}
