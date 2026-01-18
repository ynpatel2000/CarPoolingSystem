namespace Carpooling.Application.Interfaces;
public interface IAuditLogService
{
    void Log(string action, Guid actorUserId, string description);
}
