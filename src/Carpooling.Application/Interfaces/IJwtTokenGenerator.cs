using Carpooling.Domain.Entities;

namespace Carpooling.Application.Interfaces;

/// <summary>
/// Responsible for generating JWT access tokens
/// </summary>
public interface IJwtTokenGenerator
{
    string Generate(User user);
}
