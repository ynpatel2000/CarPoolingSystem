namespace Carpooling.Application.DTOs.Booking;

public record BookingResponseDto
{
    public Guid BookingId { get; init; }
    public Guid RideId { get; init; }
    public DateTime CreatedAt { get; init; }
}
