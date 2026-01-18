using Carpooling.Application.DTOs.Booking;
using Carpooling.Domain.Entities;

public interface IBookingService
{
    void BookRide(Guid passengerId, CreateBookingDto dto);
    List<Booking> GetMyBookings(Guid passengerId);
    void CancelBooking(Guid bookingId, Guid passengerId);
}
