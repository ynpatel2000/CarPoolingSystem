using Carpooling.Domain.Common;

namespace Carpooling.Domain.Entities;

public class Ride : BaseEntity
{
    public Guid DriverId { get; set; }

    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;

    public DateTime RideDate { get; set; }

    public int AvailableSeats { get; set; }

    public decimal PricePerSeat { get; set; }
}
