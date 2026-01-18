namespace Carpooling.Domain.Common;

public class AuditLog : BaseEntity
{
    public Guid ActorUserId { get; set; }
    public string Action { get; set; } = "";
    public string Description { get; set; } = "";
}

