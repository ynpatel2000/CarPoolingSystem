using Carpooling.Application.Common.Pagination;
using Carpooling.Application.DTOs.Booking;
using Carpooling.Application.Exceptions;
using Carpooling.Application.Interfaces;
using Carpooling.Domain.Entities;
using Carpooling.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Carpooling.Application.Services;

public class BookingService : IBookingService
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogService _audit;
    private readonly IBookingEventPublisher _publisher;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IAppDbContext db,
        IAuditLogService audit,
        IBookingEventPublisher publisher,
        ILogger<BookingService> logger)
    {
        _db = db;
        _audit = audit;
        _publisher = publisher;
        _logger = logger;
    }

    public void BookRide(Guid passengerId, CreateBookingDto dto)
    {
        var ride = _db.Rides.FirstOrDefault(r => r.Id == dto.RideId && !r.IsDeleted);
        if (ride == null)
            throw new AppException("Ride not found", 404);

        if (ride.AvailableSeats <= 0)
            throw new AppException("No seats available");

        if (ride.DriverId == passengerId)
            throw new AppException("Driver cannot book own ride");

        ride.AvailableSeats--;
        ride.UpdatedAt = DateTime.UtcNow;

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            RideId = ride.Id,
            PassengerId = passengerId,
            Status = BookingStatus.Confirmed
        };

        _db.Add(booking);
        _db.Update(ride);
        _db.SaveChanges();

        _audit.Log("BOOK_RIDE", passengerId, $"Booked ride {ride.Id}");

        try
        {
            _publisher.PublishBookingCreated(
                booking.Id,
                ride.Id,
                passengerId,
                "user@example.com"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "RabbitMQ publish failed for BookingId {BookingId}",
                booking.Id
            );
        }
    }

    public PagedResult<Booking> GetMyBookings(
        Guid passengerId,
        PagedRequest request)
    {
        return _db.Bookings
            .Where(b => b.PassengerId == passengerId && !b.IsDeleted)
            .OrderByDescending(b => b.CreatedAt)
            .ToPagedResult(request);
    }

    public void CancelBooking(Guid bookingId, Guid passengerId)
    {
        var booking = _db.Bookings
            .FirstOrDefault(b => b.Id == bookingId && !b.IsDeleted);

        if (booking == null)
            throw new AppException("Booking not found", 404);

        if (booking.PassengerId != passengerId)
            throw new AppException("Forbidden", 403);

        booking.Status = BookingStatus.Cancelled;
        booking.IsDeleted = true;
        booking.UpdatedAt = DateTime.UtcNow;

        _db.Update(booking);
        _db.SaveChanges();

        _audit.Log(
            "CANCEL_BOOKING",
            passengerId,
            $"Cancelled booking {bookingId}"
        );
    }
}
