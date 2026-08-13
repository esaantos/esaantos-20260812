
using System.Security.Claims;
using GestaoColaboradores.API.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result is null)
        {
            return Unauthorized(new { error = "Credenciais inválidas ou usuário inativo" });
        }

        return Ok(result);
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var login = User.FindFirstValue(ClaimTypes.Name);
        var status = User.FindFirstValue("status");

        return Ok(new { id, login, status });
    }
}
