namespace Carpooling.Application.DTOs.Auth;

public record AuthResponseDto
{
    public string Token { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
