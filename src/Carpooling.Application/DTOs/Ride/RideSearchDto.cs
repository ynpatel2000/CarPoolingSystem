namespace Carpooling.Application.DTOs.Ride;

public class RideSearchDto
{
    public string FromCity { get; set; } = string.Empty;

    public string ToCity { get; set; } = string.Empty;

    public DateTime RideDate { get; set; }

    public int RequiredSeats { get; set; } = 1;
}
