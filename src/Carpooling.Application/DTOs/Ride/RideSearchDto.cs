namespace Carpooling.Application.DTOs.Ride;

public record RideSearchDto
{
    public string FromCity { get; init; } = string.Empty;
    public string ToCity { get; init; } = string.Empty;
    public DateTime RideDate { get; init; }
    public int RequiredSeats { get; init; } = 1;
}
