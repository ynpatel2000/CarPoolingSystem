using Carpooling.Application.DTOs.Booking;
using Carpooling.Application.Exceptions;
using Carpooling.Application.Interfaces;
using Carpooling.Domain.Entities;
using Carpooling.Domain.Enums;

namespace Carpooling.Application.Services;

public class BookingService : IBookingService
{
    private readonly IAppDbContext _db;

    public BookingService(IAppDbContext db)
    {
        _db = db;
    }

    public void BookRide(Guid passengerId, CreateBookingDto dto)
    {
        var ride = _db.Rides.FirstOrDefault(x => x.Id == dto.RideId);
        if (ride == null)
            throw new AppException("Ride not found", 404);

        if (ride.AvailableSeats <= 0)
            throw new AppException("No seats available");

        if (ride.DriverId == passengerId)
            throw new AppException("Driver cannot book own ride");

        ride.AvailableSeats--;

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            RideId = dto.RideId,
            PassengerId = passengerId,
            Status = BookingStatus.Confirmed
        };

        _db.Bookings.Add(booking);
        _db.SaveChanges();
    }
}
