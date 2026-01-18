using Carpooling.Domain.Common;
using Carpooling.Domain.Enums;

namespace Carpooling.Domain.Entities;
public class Booking : BaseEntity
{
    public Guid RideId { get; set; }
    public Guid PassengerId { get; set; }
    public BookingStatus Status { get; set; }
}
