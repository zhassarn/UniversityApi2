namespace UniversityApi.Services.Interfaces;

/// <summary>Ағымдағы HTTP-сұраныстың пайдаланушысы (JWT claims негізінде).</summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsTeacher { get; }
    bool IsStudent { get; }
    int? StudentId { get; }
    int? TeacherId { get; }
}
