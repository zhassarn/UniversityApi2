using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

/// <summary>
/// API v2: курстар туралы кеңейтілген ақпарат (оқытушы + жазылған студенттер саны).
/// Бизнес-логика v1-мен ортақ ICourseService арқылы жүреді, тек DTO басқа.
/// Өзгерту/өшіру операциялары v1-де қалады.
/// </summary>
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/courses")]
[AllowAnonymous]
public class CoursesV2Controller : ApiControllerBase
{
    private readonly ICourseService _service;

    public CoursesV2Controller(ICourseService service) => _service = service;

    /// <summary>Курстар тізімі (v2). Сүзгілер: name, teacherId, minCredits, maxCredits</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<IEnumerable<CourseV2Dto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? name,
        [FromQuery] int? teacherId,
        [FromQuery] int? minCredits,
        [FromQuery] int? maxCredits)
        => Success(await _service.GetAllV2Async(name, teacherId, minCredits, maxCredits));

    /// <summary>Курсты ID бойынша алу (v2)</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReturnResult<CourseV2Dto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id) => Success(await _service.GetByIdV2Async(id));
}
