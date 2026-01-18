using Carpooling.Domain.Common;
using Carpooling.Domain.Enums;

namespace Carpooling.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    public bool IsBlocked { get; set; } = false;
}
