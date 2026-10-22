namespace UniversityApi.DTO.Courses;

public class TeacherSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

/// <summary>API v2 контракты: оқытушы туралы мәлімет және жазылған студенттер саны қосылған.</summary>
public class CourseV2Dto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public DateTime CreatedAt { get; set; }
    public TeacherSummaryDto? Teacher { get; set; }
    public int EnrolledStudentsCount { get; set; }
}
