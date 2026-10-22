using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Auth;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiVersionNeutral]
[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service) => _service = service;

    /// <summary>Тіркелу. Жаңа пайдаланушы тек Student рөлін алады.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = await _service.RegisterAsync(dto);
        return CreatedWith(nameof(Me), new { }, user);
    }

    /// <summary>Кіру: дұрыс логин/парольге JWT access token беріледі.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReturnResult<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto) => Success(await _service.LoginAsync(dto));

    /// <summary>Ағымдағы пайдаланушы туралы ақпарат</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me() => Success(await _service.GetCurrentUserAsync());
}
