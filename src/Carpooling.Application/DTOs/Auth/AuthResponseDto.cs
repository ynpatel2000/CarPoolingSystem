namespace Carpooling.Application.DTOs.Auth;

public record AuthResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}
