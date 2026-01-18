using Carpooling.Domain.Entities;

namespace Carpooling.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string Generate(User user);
}
