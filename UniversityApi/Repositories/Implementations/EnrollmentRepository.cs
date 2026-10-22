using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<IReadOnlyList<Enrollment>> GetAllAsync()
        => await Set.AsNoTracking()
            .Include(e => e.Student).Include(e => e.Course)
            .OrderBy(e => e.Id).ToListAsync();

    public override async Task<Enrollment?> GetByIdAsync(int id)
        => await Set.Include(e => e.Student).Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyList<Enrollment>> GetFilteredAsync(int? studentId, int? teacherId)
    {
        var query = Set.AsNoTracking()
            .Include(e => e.Student).Include(e => e.Course)
            .AsQueryable();

        if (studentId.HasValue) query = query.Where(e => e.StudentId == studentId.Value);
        if (teacherId.HasValue) query = query.Where(e => e.Course.TeacherId == teacherId.Value);

        return await query.OrderBy(e => e.Id).ToListAsync();
    }

    public async Task<bool> IsEnrolledAsync(int studentId, int courseId)
        => await Set.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
}
