namespace UniversityApi.Common;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";

    public static readonly string[] All = { Admin, Teacher, Student };
}

public static class AppPolicies
{
    public const string AdminOnly = "AdminOnly";           
    public const string ManageGrades = "ManageGrades";      
    public const string StudentOrAdmin = "StudentOrAdmin"; 
}

/// <summary>Короткие имена claim-ов внутри JWT.</summary>
public static class AppClaims
{
    public const string Sub = "sub";
    public const string Name = "name";
    public const string Role = "role";
    public const string StudentId = "studentId";
    public const string TeacherId = "teacherId";
}

public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    /// <summary>Ключ подписи. НЕ хранится в репозитории: user-secrets / переменная окружения Jwt__Key.</summary>
    public string Key { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 30;
}
