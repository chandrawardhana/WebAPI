using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Saga.WebApi.Infrastructures.Models;

[Route("wongnormal/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request.Username == "admin" && request.Password == "password123") 
        {
            var token = _authService.GenerateToken(request.Username);
            var employeeKey = "a12582af-59e6-4753-826d-deb7490e2442";
            return Ok(new { employeeKey, Token = token });
        }
        return Unauthorized("Username atau password salah!");
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token))
            return BadRequest("Token tidak ditemukan!");

        _authService.RevokeToken(token);
        return Ok("Logout berhasil, token dibatalkan.");
    }

    [Authorize]
    [HttpGet("check-token")]
    public IActionResult CheckToken()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (_authService.IsTokenRevoked(token))
            return Unauthorized("Token telah dibatalkan!");

        return Ok("Token masih valid.");
    }
}


