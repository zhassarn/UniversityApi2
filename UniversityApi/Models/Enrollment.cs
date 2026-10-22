using System.ComponentModel.DataAnnotations;

namespace UniversityApi.Models;

public class Enrollment
{
    [Key]
    public int Id { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    /// <summary>Баға (0–100). null — әлі қойылмаған.</summary>
    public double? Grade { get; set; }
}
