using Carpooling.Application.Common.Pagination;
using Carpooling.Application.Common.Querying;
using Carpooling.Application.DTOs.Ride;
using Carpooling.Domain.Entities;

namespace Carpooling.Application.Interfaces;

public interface IRideService
{
    void CreateRide(Guid driverId, CreateRideDto dto);

    Ride GetById(Guid rideId);

    PagedResult<Ride> GetMyRides(
        Guid driverId,
        PagedRequest page,
        SortRequest sort
    );

    PagedResult<Ride> Search(
        RideSearchDto dto,
        PagedRequest page,
        SortRequest sort
    );

    void UpdateRide(Guid rideId, Guid driverId, UpdateRideDto dto);

    void DeleteRide(Guid rideId, Guid userId, bool isAdmin);
}
