using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Models;

namespace UniversityApi.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    // Fluent API: байланыстар, FK, индекстер, delete behavior.
    // (Primary Key, Required, MaxLength — модельдердегі Data Annotations арқылы берілген.)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Identity кестелері

        modelBuilder.Entity<Student>()
            .HasIndex(s => s.Email)
            .IsUnique();

        modelBuilder.Entity<Teacher>()
            .HasIndex(t => t.Email)
            .IsUnique();

        // Teacher 1 — * Course. Мұғалімде курс болса, оны өшіруге болмайды (Restrict).
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Teacher)
            .WithMany(t => t.Courses)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        // Student * — * Course (Enrollment — байланыстырушы кесте)
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Бір студент бір курсқа бір рет қана жазыла алады (DB деңгейінде де қорғаныс)
        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();

        // Identity пайдаланушысы <-> Student / Teacher (бір аккаунт — бір студент/оқытушы)
        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.HasOne(u => u.Student).WithMany().HasForeignKey(u => u.StudentId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(u => u.Teacher).WithMany().HasForeignKey(u => u.TeacherId).OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(u => u.StudentId).IsUnique();
            b.HasIndex(u => u.TeacherId).IsUnique();
        });
    }
}
