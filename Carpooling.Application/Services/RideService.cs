using Carpooling.Application.DTOs.Ride;
using Carpooling.Application.Exceptions;
using Carpooling.Application.Interfaces;
using Carpooling.Domain.Entities;

namespace Carpooling.Application.Services;

public class RideService : IRideService
{
    private readonly IAppDbContext _db;

    public RideService(IAppDbContext db)
    {
        _db = db;
    }

    // =====================================================
    // GET ALL RIDES (ADMIN USE)
    // =====================================================
    public List<Ride> GetAll()
    {
        return _db.Rides
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }

    // =====================================================
    // GET RIDE BY ID (DETAIL VIEW)
    // =====================================================
    public Ride GetById(Guid rideId)
    {
        var ride = _db.Rides.FirstOrDefault(r => r.Id == rideId && !r.IsDeleted);

        if (ride == null)
            throw new AppException("Ride not found", 404);

        return ride;
    }

    // =====================================================
    // SEARCH RIDES (PASSENGER)
    // =====================================================
    public List<Ride> Search(RideSearchDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FromCity))
            throw new AppException("FromCity is required");

        if (string.IsNullOrWhiteSpace(dto.ToCity))
            throw new AppException("ToCity is required");

        if (dto.RequiredSeats <= 0)
            throw new AppException("Required seats must be greater than zero");

        return _db.Rides
            .Where(r =>
                r.FromCity == dto.FromCity &&
                r.ToCity == dto.ToCity &&
                r.RideDate.Date == dto.RideDate.Date &&
                r.AvailableSeats >= dto.RequiredSeats)
            .OrderBy(r => r.RideDate)
            .ToList();
    }

    // =====================================================
    // GET MY RIDES (USER AS DRIVER)
    // =====================================================
    public List<Ride> GetMyRides(Guid driverId)
    {
        return _db.Rides
            .Where(r => r.DriverId == driverId)
            .OrderByDescending(r => r.RideDate)
            .ToList();
    }

    // =====================================================
    // CREATE RIDE (USER ACTS AS DRIVER)
    // =====================================================
    public void CreateRide(Guid driverId, CreateRideDto dto)
    {
        if (dto.AvailableSeats <= 0)
            throw new AppException("Seats must be greater than zero");

        if (dto.RideDate.Date < DateTime.UtcNow.Date)
            throw new AppException("Ride date cannot be in the past");

        var ride = new Ride
        {
            Id = Guid.NewGuid(),
            DriverId = driverId,
            FromCity = dto.FromCity.Trim(),
            ToCity = dto.ToCity.Trim(),
            RideDate = dto.RideDate,
            AvailableSeats = dto.AvailableSeats,
            PricePerSeat = dto.PricePerSeat
        };

        _db.Add(ride);
        _db.SaveChanges();
    }

    // =====================================================
    // UPDATE RIDE (ONLY DRIVER CAN UPDATE)
    // =====================================================
    public void UpdateRide(Guid rideId, Guid driverId, UpdateRideDto dto)
    {
        var ride = _db.Rides.FirstOrDefault(r => r.Id == rideId);

        if (ride == null)
            throw new AppException("Ride not found", 404);

        if (ride.DriverId != driverId)
            throw new AppException("You are not allowed to update this ride", 403);

        if (dto.AvailableSeats <= 0)
            throw new AppException("Seats must be greater than zero");

        ride.FromCity = dto.FromCity.Trim();
        ride.ToCity = dto.ToCity.Trim();
        ride.RideDate = dto.RideDate;
        ride.AvailableSeats = dto.AvailableSeats;
        ride.PricePerSeat = dto.PricePerSeat;

        _db.SaveChanges();
    }

    // =====================================================
    // DELETE RIDE
    // DRIVER: delete own ride
    // ADMIN: delete any ride
    // =====================================================
    public void DeleteRide(Guid rideId, Guid userId, bool isAdmin)
    {
        var ride = _db.Rides.FirstOrDefault(r => r.Id == rideId);

        if (ride == null)
            throw new AppException("Ride not found", 404);

        if (!isAdmin && ride.DriverId != userId)
            throw new AppException("You are not allowed to delete this ride", 403);

        ride.IsDeleted = true;
        _db.Update(ride);
        _db.SaveChanges();
    }
}
