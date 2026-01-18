namespace Carpooling.Application.DTOs.Ride;

public class UpdateRideDto
{
    public string FromCity { get; set; } = string.Empty;

    public string ToCity { get; set; } = string.Empty;

    public DateTime RideDate { get; set; }

    public int AvailableSeats { get; set; }

    public decimal PricePerSeat { get; set; }
}
