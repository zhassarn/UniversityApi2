using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiVersion("1.0")]
[Route("api/courses")]                           // бастапқы URL (1-тапсырма) жұмыс істей береді
[Route("api/v{version:apiVersion}/courses")]    // нұсқаланған URL
[Authorize]
[ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status403Forbidden)]
public class CoursesController : ApiControllerBase
{
    private readonly ICourseService _service;

    public CoursesController(ICourseService service) => _service = service;

    /// <summary>Курстар тізімі. Сүзгілер: name, teacherId, minCredits, maxCredits</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReturnResult<IEnumerable<CourseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? name,
        [FromQuery] int? teacherId,
        [FromQuery] int? minCredits,
        [FromQuery] int? maxCredits)
        => Success(await _service.GetAllAsync(name, teacherId, minCredits, maxCredits));

    /// <summary>Курсты ID бойынша алу</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id) => Success(await _service.GetByIdAsync(id));

    /// <summary>Жаңа курс жасау (оқытушы міндетті түрде бар болуы керек)</summary>
    [HttpPost]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CourseCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedWith(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Курсты өзгерту</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CourseUpdateDto dto)
        => Success(await _service.UpdateAsync(id, dto));

    /// <summary>Курсты өшіру</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return SuccessNoContent();
    }
}
