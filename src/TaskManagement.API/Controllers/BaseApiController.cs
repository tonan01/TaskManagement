using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Models;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public abstract class BaseApiController : ControllerBase
    {
        protected ActionResult<ApiResponse<T>> Success<T>(T data, string message = "Success")
        {
            return Ok(ApiResponse<T>.SuccessResult(data, message));
        }

        protected ActionResult<ApiResponse> Success(string message = "Success")
        {
            return Ok(ApiResponse.SuccessResult(message));
        }

        protected ActionResult<ApiResponse> BadRequest(string message, List<string>? errors = null)
        {
            return BadRequest(ApiResponse.FailureResult(message, errors));
        }
    }
}
