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

    // =====================================================
    // REGISTER
    // =====================================================
    public AuthResponseDto Register(RegisterDto dto)
    {
        if (_db.Users.Any(x => x.Email == dto.Email))
            throw new AppException("Email already exists", 409);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.User,
            IsBlocked = false
        };

        _db.Add(user);

        // -----------------------------
        // CREATE REFRESH TOKEN
        // -----------------------------
        var refreshToken = CreateRefreshToken(user.Id);
        _db.Add(refreshToken);

        _db.SaveChanges();

        return new AuthResponseDto
        {
            AccessToken = _jwt.Generate(user),
            RefreshToken = refreshToken.Token
        };
    }

    // =====================================================
    // LOGIN
    // =====================================================
    public AuthResponseDto Login(LoginDto dto)
    {
        var user = _db.Users
            .FirstOrDefault(x => x.Email == dto.Email.Trim().ToLower());

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new AppException("Invalid credentials", 401);

        if (user.IsBlocked)
            throw new AppException("User is blocked by admin", 403);

        // -----------------------------
        // CREATE NEW REFRESH TOKEN
        // -----------------------------
        var refreshToken = CreateRefreshToken(user.Id);
        _db.Add(refreshToken);
        _db.SaveChanges();

        return new AuthResponseDto
        {
            AccessToken = _jwt.Generate(user),
            RefreshToken = refreshToken.Token
        };
    }

    // =====================================================
    // REFRESH TOKEN
    // =====================================================
    public AuthResponseDto RefreshToken(string refreshToken)
    {
        var token = _db.RefreshTokens
            .FirstOrDefault(x =>
                x.Token == refreshToken &&
                !x.IsRevoked &&
                x.ExpiresAt > DateTime.UtcNow);

        if (token == null)
            throw new AppException("Invalid or expired refresh token", 401);

        var user = _db.Users.FirstOrDefault(x => x.Id == token.UserId);

        if (user == null || user.IsBlocked)
            throw new AppException("User not allowed", 403);

        // -----------------------------
        // ROTATE REFRESH TOKEN
        // -----------------------------
        token.IsRevoked = true;

        var newRefreshToken = CreateRefreshToken(user.Id);

        _db.Add(newRefreshToken);
        _db.SaveChanges();

        return new AuthResponseDto
        {
            AccessToken = _jwt.Generate(user),
            RefreshToken = newRefreshToken.Token
        };
    }

    // =====================================================
    // PRIVATE HELPERS
    // =====================================================
    private static RefreshToken CreateRefreshToken(Guid userId)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false
        };
    }
}
