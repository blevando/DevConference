using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaistringsAPI.Managers;
using NaistringsAPI.Models.DTOs;

namespace NaistringsAPI.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthManager _manager;
    public AuthController(AuthManager manager) => _manager = manager;

    [AllowAnonymous, HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _manager.RegisterAsync(request);
        return result.Succeeded ? StatusCode(StatusCodes.Status201Created, new { message = "Account created. Email confirmation is required before login." })
            : BadRequest(new { errors = result.Errors.Select(x => x.Description).ToArray() });
    }

    [AllowAnonymous, HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _manager.LoginAsync(request);
        return result is null ? Unauthorized(new { message = "Invalid credentials or account not confirmed." }) : Ok(result);
    }

    [Authorize, HttpGet("me")]
    public IActionResult Me() => Ok(new { userId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value, email = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value, roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(x => x.Value).ToArray() });

    [Authorize(Roles = "SuperAdmin,Admin"), HttpGet("admin-check")]
    public IActionResult AdminCheck() => Ok(new { message = "Administrative access granted." });
}
