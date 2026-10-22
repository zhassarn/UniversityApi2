using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Auth;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

/// <summary>Пайдаланушылар мен рөлдерді басқару — тек Admin.</summary>
[ApiVersionNeutral]
[Route("api/users")]
[Authorize(Policy = AppPolicies.AdminOnly)]
[ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status403Forbidden)]
public class UsersController : ApiControllerBase
{
    private readonly IUserAdminService _service;

    public UsersController(IUserAdminService service) => _service = service;

    /// <summary>Барлық пайдаланушылар (құпия деректерсіз)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() => Success(await _service.GetAllAsync());

    /// <summary>Пайдаланушыға рөл тағайындау (Admin / Teacher / Student)</summary>
    [HttpPut("{id}/role")]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetRole(string id, [FromBody] UserRoleDto dto)
        => Success(await _service.SetRoleAsync(id, dto));

    /// <summary>Аккаунтты Student / Teacher сущностіне байлау (екеуін де null қылса — байланыс алынады)</summary>
    [HttpPut("{id}/link")]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Link(string id, [FromBody] UserLinkDto dto)
        => Success(await _service.LinkAsync(id, dto));
}
