using Carpooling.Application.Interfaces;
using Carpooling.Domain.Common;

namespace Carpooling.Application.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IAppDbContext _db;

    public AuditLogService(IAppDbContext db)
    {
        _db = db;
    }

    public void Log(string action, Guid userId, string description)
    {
        var audit = new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = userId,
            Action = action,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        _db.Add(audit);
        _db.SaveChanges();
    }
}
