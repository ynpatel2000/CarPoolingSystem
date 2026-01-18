using Carpooling.Domain.Enums;

namespace Carpooling.Application.DTOs.Admin;

public class BookingResponseDto
{
    public Guid Id { get; set; }
    public Guid RideId { get; set; }
    public Guid PassengerId { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
