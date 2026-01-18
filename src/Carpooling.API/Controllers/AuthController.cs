using Carpooling.Application.DTOs.Auth;
using Carpooling.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Carpooling.API.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    // =====================================================
    // REGISTER
    // =====================================================
    // POST: api/v1/auth/register
    // =====================================================
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public IActionResult Register(
        [FromBody] RegisterDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "User registration attempt for Email={Email}",
            dto.Email
        );

        var result = _authService.Register(dto);

        _logger.LogInformation(
            "User registered successfully for Email={Email}",
            dto.Email
        );

        return Ok(result);
    }

    // =====================================================
    // LOGIN
    // =====================================================
    // POST: api/v1/auth/login
    // =====================================================
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public IActionResult Login(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Login attempt for Email={Email}",
            dto.Email
        );

        var result = _authService.Login(dto);

        _logger.LogInformation(
            "Login successful for Email={Email}",
            dto.Email
        );

        return Ok(result);
    }

    // =====================================================
    // REFRESH TOKEN
    // =====================================================
    // POST: api/v1/auth/refresh
    // =====================================================
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public IActionResult Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            _logger.LogWarning("Refresh token request with empty token");
            return BadRequest("Refresh token is required");
        }

        var result = _authService.RefreshToken(request.RefreshToken);

        _logger.LogInformation("Refresh token issued successfully");

        return Ok(result);
    }
}