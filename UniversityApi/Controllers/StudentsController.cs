using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Students;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiVersion("1.0")]
[Route("api/students")]                           // бастапқы URL (1-тапсырма) жұмыс істей береді
[Route("api/v{version:apiVersion}/students")]    // нұсқаланған URL
[Authorize]
[ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status403Forbidden)]
public class StudentsController : ApiControllerBase
{
    private readonly IStudentService _service;

    public StudentsController(IStudentService service) => _service = service;

    /// <summary>Барлық студенттер тізімі</summary>
    [HttpGet]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<IEnumerable<StudentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() => Success(await _service.GetAllAsync());

    /// <summary>Студентті ID бойынша алу</summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = AppPolicies.StudentOrAdmin)]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id) => Success(await _service.GetByIdAsync(id));

    /// <summary>Студентті өзі жазылған курстарымен бірге алу</summary>
    [HttpGet("{id:int}/courses")]
    [Authorize(Policy = AppPolicies.StudentOrAdmin)]
    [ProducesResponseType(typeof(ReturnResult<StudentWithCoursesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWithCourses(int id) => Success(await _service.GetWithCoursesAsync(id));

    /// <summary>Жаңа студент жасау</summary>
    [HttpPost]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] StudentCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedWith(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Студент деректерін өзгерту</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] StudentUpdateDto dto)
        => Success(await _service.UpdateAsync(id, dto));

    /// <summary>Студентті өшіру</summary>
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
