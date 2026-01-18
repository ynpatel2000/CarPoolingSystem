using Carpooling.Application.Common.Pagination;
using Carpooling.Application.DTOs.Booking;
using Carpooling.Domain.Entities;

namespace Carpooling.Application.Interfaces;

public interface IBookingService
{
    void BookRide(Guid passengerId, CreateBookingDto dto);

    PagedResult<Booking> GetMyBookings(
        Guid passengerId,
        PagedRequest request
    );

    void CancelBooking(Guid bookingId, Guid passengerId);
}
