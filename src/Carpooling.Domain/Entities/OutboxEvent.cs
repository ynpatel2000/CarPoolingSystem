using Carpooling.Domain.Common;

namespace Carpooling.Domain.Entities;

public class OutboxEvent : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;

    public bool IsProcessed { get; set; } = false;
    public DateTime? ProcessedAt { get; set; }
}
