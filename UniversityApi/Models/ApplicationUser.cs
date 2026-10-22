using Microsoft.AspNetCore.Identity;

namespace UniversityApi.Models;

/// <summary>Identity пайдаланушысы. Студент/оқытушы аккаунты университет сущностіне байланады.</summary>
public class ApplicationUser : IdentityUser
{
    public int? StudentId { get; set; }
    public Student? Student { get; set; }

    public int? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
}
