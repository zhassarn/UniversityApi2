using AutoMapper;
using UniversityApi.DTO.Auth;
using UniversityApi.DTO.Courses;
using UniversityApi.DTO.Enrollments;
using UniversityApi.DTO.Students;
using UniversityApi.DTO.Teachers;
using UniversityApi.Models;

namespace UniversityApi.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Student
        CreateMap<Student, StudentDto>();
        CreateMap<StudentCreateDto, Student>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore());
        CreateMap<StudentUpdateDto, Student>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore());
        CreateMap<Enrollment, StudentCourseDto>()
            .ForMember(d => d.EnrollmentId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course.Name))
            .ForMember(d => d.Credits, o => o.MapFrom(s => s.Course.Credits));
        CreateMap<Student, StudentWithCoursesDto>()
            .ForMember(d => d.Courses, o => o.MapFrom(s => s.Enrollments));

        // Teacher
        CreateMap<Teacher, TeacherDto>();
        CreateMap<TeacherCreateDto, Teacher>().ForMember(d => d.Id, o => o.Ignore());
        CreateMap<TeacherUpdateDto, Teacher>().ForMember(d => d.Id, o => o.Ignore());

        // Course
        CreateMap<Course, CourseDto>()
            .ForMember(d => d.TeacherName, o => o.MapFrom(s =>
                s.Teacher != null ? s.Teacher.FirstName + " " + s.Teacher.LastName : null));
        CreateMap<CourseCreateDto, Course>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore());
        CreateMap<CourseUpdateDto, Course>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore());

        // Enrollment
        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Student.FirstName + " " + s.Student.LastName))
            .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course.Name));

        // Course v2
        CreateMap<Teacher, TeacherSummaryDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));
        CreateMap<Course, CourseV2Dto>()
            .ForMember(d => d.EnrolledStudentsCount, o => o.MapFrom(s => s.Enrollments.Count));

        // Identity user (рөлдер сервисте толтырылады)
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(d => d.Roles, o => o.Ignore());
    }
}
