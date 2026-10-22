using Microsoft.AspNetCore.Mvc;
using UniversityClient.Models;
using UniversityClient.Services;

namespace UniversityClient.Controllers;

public class CoursesController : Controller
{
    private readonly IUniversityApiClient _api;
    private readonly IConfiguration _config;

    public CoursesController(IUniversityApiClient api, IConfiguration config)
    {
        _api = api;
        _config = config;
    }

    // GET /Courses?version=v1|v2
    public async Task<IActionResult> Index(string? version, CancellationToken ct)
    {
        version = ResolveVersion(version);
        var result = await _api.GetCoursesAsync(version, ct);

        return View(new CoursesIndexViewModel
        {
            Version = version,
            Courses = result.Data ?? new List<ClientCourse>(),
            Error = result.IsSuccess ? null : result.ErrorMessage
        });
    }

    // GET /Courses/Details/5?version=v1|v2
    public async Task<IActionResult> Details(int id, string? version, CancellationToken ct)
    {
        version = ResolveVersion(version);
        var result = await _api.GetCourseAsync(id, version, ct);

        if (!result.IsSuccess && result.StatusCode == 404) Response.StatusCode = 404;

        return View(new CourseDetailsViewModel
        {
            Version = version,
            Course = result.Data,
            Error = result.IsSuccess ? null : result.ErrorMessage
        });
    }

    // Браузерден (JavaScript) API-ға тікелей сұраныс: CORS саясатын көрсету үшін
    public IActionResult CorsDemo()
        => View(new CorsDemoViewModel { ApiBaseUrl = (_config["UniversityApi:BaseUrl"] ?? string.Empty).TrimEnd('/') });

    private string ResolveVersion(string? version)
        => UniversityApiClient.NormalizeVersion(version ?? _config["UniversityApi:DefaultVersion"]);
}
