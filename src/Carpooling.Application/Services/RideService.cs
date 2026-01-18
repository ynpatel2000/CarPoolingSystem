using Carpooling.Application.Common.Pagination;
using Carpooling.Application.Common.Querying;
using Carpooling.Application.DTOs.Ride;
using Carpooling.Application.Exceptions;
using Carpooling.Application.Interfaces;
using Carpooling.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Carpooling.Application.Services;

public class RideService : IRideService
{
    private readonly IAppDbContext _db;
    private readonly ILogger<RideService> _logger;

    public RideService(IAppDbContext db, ILogger<RideService> logger)
    {
        _db = db;
        _logger = logger;
    }

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

        _logger.LogInformation("Ride {RideId} created by {DriverId}", ride.Id, driverId);
    }

    public Ride GetById(Guid rideId)
    {
        return _db.Rides.FirstOrDefault(r => r.Id == rideId && !r.IsDeleted)
               ?? throw new AppException("Ride not found", 404);
    }

    public PagedResult<Ride> GetMyRides(
        Guid driverId,
        PagedRequest page,
        SortRequest sort)
    {
        return _db.Rides
            .Where(r => r.DriverId == driverId && !r.IsDeleted)
            .ApplySorting(sort, r => r.CreatedAt)
            .ToPagedResult(page);
    }

    public PagedResult<Ride> Search(
        RideSearchDto dto,
        PagedRequest page,
        SortRequest sort)
    {
        return _db.Rides
            .Where(r =>
                !r.IsDeleted &&
                r.FromCity == dto.FromCity &&
                r.ToCity == dto.ToCity &&
                r.RideDate.Date == dto.RideDate.Date &&
                r.AvailableSeats >= dto.RequiredSeats)
            .ApplySorting(sort, r => r.RideDate)
            .ToPagedResult(page);
    }

    public void UpdateRide(Guid rideId, Guid driverId, UpdateRideDto dto)
    {
        var ride = GetById(rideId);

        if (ride.DriverId != driverId)
            throw new AppException("Forbidden", 403);

        ride.FromCity = dto.FromCity.Trim();
        ride.ToCity = dto.ToCity.Trim();
        ride.RideDate = dto.RideDate;
        ride.AvailableSeats = dto.AvailableSeats;
        ride.PricePerSeat = dto.PricePerSeat;
        ride.UpdatedAt = DateTime.UtcNow;

        _db.Update(ride);
        _db.SaveChanges();
    }

    public void DeleteRide(Guid rideId, Guid userId, bool isAdmin)
    {
        var ride = GetById(rideId);

        if (!isAdmin && ride.DriverId != userId)
            throw new AppException("Forbidden", 403);

        ride.IsDeleted = true;
        ride.UpdatedAt = DateTime.UtcNow;

        _db.Update(ride);
        _db.SaveChanges();
    }
}
