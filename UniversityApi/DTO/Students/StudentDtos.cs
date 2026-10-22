using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Students;

public class StudentCreateDto
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

    [Required(ErrorMessage = "BirthDate is required")]
    public DateTime? BirthDate { get; set; }
}

public class StudentUpdateDto : StudentCreateDto { }

public class StudentDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StudentCourseDto
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int Credits { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public double? Grade { get; set; }
}

public class StudentWithCoursesDto : StudentDto
{
    public List<StudentCourseDto> Courses { get; set; } = new();
}
