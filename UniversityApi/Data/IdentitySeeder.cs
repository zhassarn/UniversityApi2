using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Common;
using UniversityApi.Models;

namespace UniversityApi.Data;

/// <summary>
/// Рөлдер мен тест пайдаланушыларын құрады (жоба іске қосылғанда, бір рет).
/// Тест пайдаланушыларының паролі репозиторийде сақталмайды: конфигурациядағы "Seed:Password" (user-secrets).
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider sp, IConfiguration config, ILogger logger)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Role created: {Role}", role);
            }
        }

        var password = config["Seed:Password"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Seed:Password is not configured: test users were not created. " +
                              "Set it with: dotnet user-secrets set \"Seed:Password\" \"<password>\"");
            return;
        }

        var db = sp.GetRequiredService<ApplicationDbContext>();
        var users = sp.GetRequiredService<UserManager<ApplicationUser>>();

        // (userName, email, role, studentEmail, teacherEmail)
        var seed = new (string UserName, string Email, string Role, string? StudentEmail, string? TeacherEmail)[]
        {
            ("admin",    "admin@uni.kz",    AppRoles.Admin,   null, null),
            ("teacher1", "teacher1@uni.kz", AppRoles.Teacher, null, "aidar.nurlanov@uni.kz"),     // курстары: 1 және 4
            ("teacher2", "teacher2@uni.kz", AppRoles.Teacher, null, "madina.sarsenova@uni.kz"),   // курсы: 3
            ("student1", "student1@uni.kz", AppRoles.Student, "zhassar@student.kz", null),
            ("student2", "student2@uni.kz", AppRoles.Student, "alex.smith@student.kz", null),
        };

        foreach (var item in seed)
        {
            if (await users.FindByNameAsync(item.UserName) is not null) continue;

            int? studentId = item.StudentEmail is null ? null
                : await db.Students.Where(s => s.Email == item.StudentEmail).Select(s => (int?)s.Id).FirstOrDefaultAsync();
            int? teacherId = item.TeacherEmail is null ? null
                : await db.Teachers.Where(t => t.Email == item.TeacherEmail).Select(t => (int?)t.Id).FirstOrDefaultAsync();

            var user = new ApplicationUser
            {
                UserName = item.UserName,
                Email = item.Email,
                EmailConfirmed = true,
                StudentId = studentId,
                TeacherId = teacherId
            };

            var result = await users.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                logger.LogError("Seed user {UserName} was not created: {Errors}",
                    item.UserName, string.Join("; ", result.Errors.Select(e => e.Code)));
                continue;
            }

            await users.AddToRoleAsync(user, item.Role);
            logger.LogInformation("Seed user created: {UserName} ({Role})", item.UserName, item.Role);
        }
    }
}
