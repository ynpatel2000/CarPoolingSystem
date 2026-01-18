namespace Carpooling.Infrastructure.Messaging.Models;

public class BookingEvent
{
    public Guid BookingId { get; set; }
    public Guid RideId { get; set; }
    public Guid PassengerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
}
