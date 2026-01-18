using Carpooling.Domain.Common;
using Carpooling.Domain.Enums;

namespace Carpooling.Domain.Entities;
public class User : BaseEntity
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsBlocked { get; set; } = false;
}
