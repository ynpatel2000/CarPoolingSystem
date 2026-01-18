namespace Carpooling.Application.DTOs.Booking;

public record CreateBookingDto
{
    public Guid RideId { get; init; }
}
