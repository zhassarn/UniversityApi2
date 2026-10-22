namespace UniversityClient.Models;

/// <summary>API-дің бірыңғай ReturnResult&lt;T&gt; жауабы.</summary>
public class ApiEnvelope<T>
{
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public T? Result { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? TraceId { get; set; }
}

/// <summary>Клиент ішіндегі нәтиже: сәтті деректер немесе түсінікті қате.</summary>
public class ApiResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public int StatusCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static ApiResult<T> Ok(T data) => new() { IsSuccess = true, Data = data, StatusCode = 200 };
    public static ApiResult<T> Fail(int statusCode, string message) => new() { IsSuccess = false, StatusCode = statusCode, ErrorMessage = message };
}

public class ClientTeacher
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Department { get; set; }
}

/// <summary>v1 және v2 жауаптарын бірге қамтитын модель (v1: TeacherName; v2: Teacher + EnrolledStudentsCount).</summary>
public class ClientCourse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public DateTime CreatedAt { get; set; }

    // v1
    public int? TeacherId { get; set; }
    public string? TeacherName { get; set; }

    // v2
    public ClientTeacher? Teacher { get; set; }
    public int? EnrolledStudentsCount { get; set; }

    public string TeacherDisplay => Teacher is null
        ? (TeacherName ?? "—")
        : $"{Teacher.FullName} ({Teacher.Department})";
}

public class CoursesIndexViewModel
{
    public string Version { get; set; } = "v2";
    public List<ClientCourse> Courses { get; set; } = new();
    public string? Error { get; set; }
}

public class CourseDetailsViewModel
{
    public string Version { get; set; } = "v2";
    public ClientCourse? Course { get; set; }
    public string? Error { get; set; }
}

public class CorsDemoViewModel
{
    public string ApiBaseUrl { get; set; } = string.Empty;
}
