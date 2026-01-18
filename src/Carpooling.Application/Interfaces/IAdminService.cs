using Carpooling.Application.Common.Pagination;
using Carpooling.Application.Common.Querying;
using Carpooling.Application.DTOs.Admin;
using Carpooling.Domain.Common;

namespace Carpooling.Application.Interfaces;

public interface IAdminService
{
    void BlockUser(Guid userId);
    void UnblockUser(Guid userId);
    void DeleteRide(Guid rideId);

    PagedResult<UserResponseDto> GetUsers(
        PagedRequest page,
        SortRequest sort,
        FilterRequest filter
    );

    PagedResult<BookingResponseDto> GetBookings(
        PagedRequest page,
        SortRequest sort
    );

    PagedResult<AuditLog> GetAuditLogs(PagedRequest request);
}
