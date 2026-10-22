using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class StudentRepository : Repository<Student>, IStudentRepository
{
    public StudentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Student?> GetWithCoursesAsync(int id)
        => await Set.AsNoTracking()
            .Include(s => s.Enrollments).ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.Id == id);

    public override async Task<bool> ExistsAsync(int id)
        => await Set.AnyAsync(x => x.Id == id);

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        var normalized = email.Trim().ToLower();
        return await Set.AnyAsync(s => s.Email.ToLower() == normalized && (excludeId == null || s.Id != excludeId));
    }
}
