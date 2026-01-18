using Carpooling.Application.DTOs.Auth;

namespace Carpooling.Application.Interfaces;

public interface IAuthService
{
    AuthResponseDto Register(RegisterDto dto);
    AuthResponseDto Login(LoginDto dto);
    AuthResponseDto RefreshToken(string refreshToken);
}
