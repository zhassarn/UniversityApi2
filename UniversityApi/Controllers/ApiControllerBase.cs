using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;

namespace UniversityApi.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected string TraceId => HttpContext.TraceIdentifier;

    protected IActionResult Success<T>(T data)
        => Ok(ReturnResult<T>.Success(data, StatusCodes.Status200OK, TraceId));

    protected IActionResult SuccessNoContent()
        => Ok(ReturnResult<object>.Success(null, StatusCodes.Status200OK, TraceId));

    protected IActionResult CreatedWith<T>(string actionName, object routeValues, T data)
        => CreatedAtAction(actionName, routeValues, ReturnResult<T>.Success(data, StatusCodes.Status201Created, TraceId));
}
