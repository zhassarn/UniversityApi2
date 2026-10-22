using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Enrollments;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiVersion("1.0")]
[Route("api/enrollments")]                           // бастапқы URL (1-тапсырма) жұмыс істей береді
[Route("api/v{version:apiVersion}/enrollments")]    // нұсқаланған URL
[Authorize]
[ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status403Forbidden)]
public class EnrollmentsController : ApiControllerBase
{
    private readonly IEnrollmentService _service;

    public EnrollmentsController(IEnrollmentService service) => _service = service;

    /// <summary>Барлық жазылулар тізімі</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<IEnumerable<EnrollmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() => Success(await _service.GetAllAsync());

    /// <summary>Жазылуды ID бойынша алу</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id) => Success(await _service.GetByIdAsync(id));

    /// <summary>Студентті курсқа жазу</summary>
    [HttpPost]
    [Authorize(Policy = AppPolicies.StudentOrAdmin)]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enroll([FromBody] EnrollmentCreateDto dto)
    {
        var created = await _service.EnrollAsync(dto);
        return CreatedWith(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Студент бағасын өзгерту</summary>
    [HttpPut("{id:int}/grade")]
    [Authorize(Policy = AppPolicies.ManageGrades)]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGrade(int id, [FromBody] EnrollmentGradeUpdateDto dto)
        => Success(await _service.UpdateGradeAsync(id, dto));

    /// <summary>Студентті курстан шығару</summary>
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
