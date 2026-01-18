using Carpooling.Domain.Enums;

namespace Carpooling.Application.DTOs.Admin;

public class UserResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsBlocked { get; set; }

    public DateTime CreatedAt { get; set; }
}
