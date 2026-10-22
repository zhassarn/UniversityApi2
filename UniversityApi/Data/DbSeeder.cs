using Microsoft.EntityFrameworkCore;
using UniversityApi.Models;

namespace UniversityApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.Teachers.AnyAsync()) return;

        var teachers = new[]
        {
            new Teacher { FirstName = "Aidar", LastName = "Nurlanov", Email = "aidar.nurlanov@uni.kz", Department = "Computer Science" },
            new Teacher { FirstName = "Madina", LastName = "Sarsenova", Email = "madina.sarsenova@uni.kz", Department = "Mathematics" },
            new Teacher { FirstName = "John", LastName = "Miller", Email = "john.miller@uni.kz", Department = "Software Engineering" }
        };
        db.Teachers.AddRange(teachers);

        var students = new[]
        {
            new Student { FirstName = "Zhassar", LastName = "Student", Email = "zhassar@student.kz", BirthDate = new DateTime(2004, 3, 15) },
            new Student { FirstName = "Alex", LastName = "Smith", Email = "alex.smith@student.kz", BirthDate = new DateTime(2003, 7, 21) },
            new Student { FirstName = "Aruzhan", LastName = "Bek", Email = "aruzhan.bek@student.kz", BirthDate = new DateTime(2004, 11, 2) }
        };
        db.Students.AddRange(students);

        var courses = new[]
        {
            new Course { Name = "ASP.NET Core Web API", Description = "REST API, EF Core, DI", Credits = 5, Teacher = teachers[0] },
            new Course { Name = "Databases", Description = "SQL, PostgreSQL, normalization", Credits = 4, Teacher = teachers[2] },
            new Course { Name = "Discrete Mathematics", Description = "Logic, graphs, combinatorics", Credits = 3, Teacher = teachers[1] },
            new Course { Name = "Algorithms", Description = "Complexity, sorting, graphs", Credits = 6, Teacher = teachers[0] }
        };
        db.Courses.AddRange(courses);

        db.Enrollments.AddRange(
            new Enrollment { Student = students[0], Course = courses[0], Grade = 95 },
            new Enrollment { Student = students[0], Course = courses[1] },
            new Enrollment { Student = students[1], Course = courses[0], Grade = 80 },
            new Enrollment { Student = students[2], Course = courses[2] }
        );

        await db.SaveChangesAsync();
    }
}
