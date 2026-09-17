using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Api.DTOs;
using PurchaseManagement.Api.Services;

namespace PurchaseManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Invalid request data";

            return BadRequest(new LoginResponseDto
            {
                Success = false,
                Message = firstError
            });
        }

        var result = await _authService.LoginAsync(request);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }
}
