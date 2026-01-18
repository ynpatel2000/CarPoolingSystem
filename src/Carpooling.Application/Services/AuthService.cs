using Carpooling.Application.DTOs.Auth;
using Carpooling.Application.Exceptions;
using Carpooling.Application.Interfaces;
using Carpooling.Domain.Entities;
using Carpooling.Domain.Enums;

namespace Carpooling.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _db;
    private readonly IJwtTokenGenerator _jwt;

    public AuthService(IAppDbContext db, IJwtTokenGenerator jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public string Register(RegisterDto dto)
    {
        if (_db.Users.Any(x => x.Email == dto.Email))
            throw new AppException("Email already exists", 409);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.User
        };

        _db.Users.Add(user);
        _db.SaveChanges();

        return _jwt.Generate(user);
    }

    public string Login(LoginDto dto)
    {
        var user = _db.Users.AsNoTracking()
            .FirstOrDefault(x => x.Email == dto.Email);

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new AppException("Invalid credentials", 401);

        if (user.IsBlocked)
            throw new AppException("User is blocked by admin", 403);

        return _jwt.Generate(user);
    }
}
