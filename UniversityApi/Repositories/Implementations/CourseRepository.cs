using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class CourseRepository : Repository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<IReadOnlyList<Course>> GetAllAsync()
        => await Set.AsNoTracking().Include(c => c.Teacher).OrderBy(c => c.Id).ToListAsync();

    public override async Task<Course?> GetByIdAsync(int id)
        => await Set.Include(c => c.Teacher).FirstOrDefaultAsync(c => c.Id == id);

    public override async Task<bool> ExistsAsync(int id)
        => await Set.AnyAsync(c => c.Id == id);

    public async Task<IReadOnlyList<Course>> SearchAsync(string? name, int? teacherId, int? minCredits, int? maxCredits)
        => await Filter(Set.AsNoTracking().Include(c => c.Teacher), name, teacherId, minCredits, maxCredits)
            .OrderBy(c => c.Id).ToListAsync();

    public async Task<IReadOnlyList<Course>> SearchWithDetailsAsync(string? name, int? teacherId, int? minCredits, int? maxCredits)
        => await Filter(Set.AsNoTracking().Include(c => c.Teacher).Include(c => c.Enrollments),
                name, teacherId, minCredits, maxCredits)
            .OrderBy(c => c.Id).ToListAsync();

    public async Task<Course?> GetByIdWithDetailsAsync(int id)
        => await Set.AsNoTracking()
            .Include(c => c.Teacher).Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == id);

    private static IQueryable<Course> Filter(IQueryable<Course> query, string? name, int? teacherId, int? minCredits, int? maxCredits)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            var pattern = $"%{name.Trim()}%";
            query = query.Where(c => EF.Functions.ILike(c.Name, pattern));
        }
        if (teacherId.HasValue) query = query.Where(c => c.TeacherId == teacherId.Value);
        if (minCredits.HasValue) query = query.Where(c => c.Credits >= minCredits.Value);
        if (maxCredits.HasValue) query = query.Where(c => c.Credits <= maxCredits.Value);
        return query;
    }
}
