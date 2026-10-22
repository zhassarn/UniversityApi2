using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Teachers;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiVersion("1.0")]
[Route("api/teachers")]                           // бастапқы URL (1-тапсырма) жұмыс істей береді
[Route("api/v{version:apiVersion}/teachers")]    // нұсқаланған URL
[Authorize]
[ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status403Forbidden)]
public class TeachersController : ApiControllerBase
{
    private readonly ITeacherService _service;

    public TeachersController(ITeacherService service) => _service = service;

    /// <summary>Барлық оқытушылар тізімі</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<IEnumerable<TeacherDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() => Success(await _service.GetAllAsync());

    /// <summary>Оқытушыны ID бойынша алу</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id) => Success(await _service.GetByIdAsync(id));

    /// <summary>Жаңа оқытушы жасау</summary>
    [HttpPost]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] TeacherCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedWith(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Оқытушы деректерін өзгерту</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] TeacherUpdateDto dto)
        => Success(await _service.UpdateAsync(id, dto));

    /// <summary>Оқытушыны өшіру (курсы болса — 409)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return SuccessNoContent();
    }
}
