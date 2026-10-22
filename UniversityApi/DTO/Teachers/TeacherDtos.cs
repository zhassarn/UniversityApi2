using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Teachers;

public class TeacherCreateDto
{
    [Required(ErrorMessage = "FirstName is required")]
    [StringLength(100, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "LastName is required")]
    [StringLength(100, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email has invalid format")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required")]
    [StringLength(150, MinimumLength = 1)]
    public string Department { get; set; } = string.Empty;
}

public class TeacherUpdateDto : TeacherCreateDto { }

public class TeacherDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}
