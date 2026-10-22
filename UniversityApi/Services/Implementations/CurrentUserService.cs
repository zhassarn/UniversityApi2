using System.Security.Claims;
using UniversityApi.Common;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class CurrentUserService : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user;

    public CurrentUserService(IHttpContextAccessor accessor) => _user = accessor.HttpContext?.User;

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated == true;
    public string? UserId => _user?.FindFirstValue(AppClaims.Sub);
    public string? UserName => _user?.FindFirstValue(AppClaims.Name);

    public bool IsAdmin => _user?.IsInRole(AppRoles.Admin) == true;
    public bool IsTeacher => _user?.IsInRole(AppRoles.Teacher) == true;
    public bool IsStudent => _user?.IsInRole(AppRoles.Student) == true;

    public int? StudentId => ParseInt(_user?.FindFirstValue(AppClaims.StudentId));
    public int? TeacherId => ParseInt(_user?.FindFirstValue(AppClaims.TeacherId));

    private static int? ParseInt(string? value) => int.TryParse(value, out var id) ? id : null;
}
