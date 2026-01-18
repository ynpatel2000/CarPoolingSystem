using Carpooling.Application.DTOs.Admin;
using Carpooling.Domain.Entities;

namespace Carpooling.Application.Mappings;

public static class UserMappings
{
    public static UserResponseDto ToDto(this User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsBlocked = user.IsBlocked,
            CreatedAt = user.CreatedAt
        };
    }
}
