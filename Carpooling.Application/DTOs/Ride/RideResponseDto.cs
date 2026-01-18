namespace Carpooling.Application.DTOs.Ride;

public record RideResponseDto
{
    public Guid Id { get; init; }
    public string FromCity { get; init; } = string.Empty;
    public string ToCity { get; init; } = string.Empty;
    public DateTime RideDate { get; init; }
    public int AvailableSeats { get; init; }
    public decimal PricePerSeat { get; init; }
}
