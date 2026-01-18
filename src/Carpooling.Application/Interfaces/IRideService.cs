using Carpooling.Application.DTOs.Ride;
using Carpooling.Domain.Entities;

namespace Carpooling.Application.Interfaces;

public interface IRideService
{
    List<Ride> GetAll();
    Ride GetById(Guid rideId);
    List<Ride> Search(RideSearchDto dto);
    List<Ride> GetMyRides(Guid driverId);
    void CreateRide(Guid driverId, CreateRideDto dto);
    void UpdateRide(Guid rideId, Guid driverId, UpdateRideDto dto);
    void DeleteRide(Guid rideId, Guid userId, bool isAdmin);
}
