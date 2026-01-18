using Carpooling.Application.DTOs.Auth;

namespace Carpooling.Application.Interfaces;

public interface IAuthService
{
    string Register(RegisterDto dto);
    string Login(LoginDto dto);
}