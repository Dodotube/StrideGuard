using Microsoft.AspNetCore.Mvc;
using StrideGuard.Application.DTOs;
using StrideGuard.Application.Services;

namespace StrideGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("registrar")]
    public async Task<ActionResult<AuthResponse>> Registrar([FromBody] RegistrarUsuarioRequest req)
    {
        var res = await authService.RegistrarAsync(req);
        return Created("", res);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        var res = await authService.LoginAsync(req);
        return Ok(res);
    }
}
