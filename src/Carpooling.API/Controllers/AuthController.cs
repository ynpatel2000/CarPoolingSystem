using Carpooling.Application.DTOs.Auth;
using Carpooling.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public IActionResult Register(RegisterDto dto)
        => Ok(_auth.Register(dto));

    [HttpPost("login")]
    public IActionResult Login(LoginDto dto)
        => Ok(_auth.Login(dto));
}
