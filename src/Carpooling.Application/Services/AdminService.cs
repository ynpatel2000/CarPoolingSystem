using Carpooling.Application.Common.Pagination;
using Carpooling.Application.Common.Querying;
using Carpooling.Application.DTOs.Admin;
using Carpooling.Application.Exceptions;
using Carpooling.Application.Interfaces;
using Carpooling.Application.Mappings;
using Carpooling.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Carpooling.Application.Services;

public class AdminService : IAdminService
{
    private readonly IAppDbContext _db;
    private readonly IAuditLogService _audit;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IAppDbContext db,
        IAuditLogService audit,
        ILogger<AdminService> logger)
    {
        _db = db;
        _audit = audit;
        _logger = logger;
    }

    public PagedResult<UserResponseDto> GetUsers(
        PagedRequest page,
        SortRequest sort,
        FilterRequest filter)
    {
        var query = _db.Users
            .ApplySearch(
                filter.Search,
                u => u.Name.Contains(filter.Search!) ||
                     u.Email.Contains(filter.Search!))
            .ApplySorting(sort, u => u.CreatedAt);

        var paged = query.ToPagedResult(page);

        return new PagedResult<UserResponseDto>
        {
            Items = paged.Items.Select(u => u.ToDto()).ToList(),
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };
    }

    public PagedResult<BookingResponseDto> GetBookings(
        PagedRequest page,
        SortRequest sort)
    {
        var paged = _db.Bookings
            .ApplySorting(sort, b => b.CreatedAt)
            .ToPagedResult(page);

        return new PagedResult<BookingResponseDto>
        {
            Items = paged.Items.Select(b => new BookingResponseDto
            {
                Id = b.Id,
                RideId = b.RideId,
                PassengerId = b.PassengerId,
                Status = b.Status,
                CreatedAt = b.CreatedAt
            }).ToList(),
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };
    }

    public PagedResult<AuditLog> GetAuditLogs(PagedRequest request)
    {
        return _db.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .ToPagedResult(request);
    }

    public void BlockUser(Guid userId)
    {
        var user = _db.Users.FirstOrDefault(u => u.Id == userId)
                   ?? throw new AppException("User not found", 404);

        user.IsBlocked = true;
        user.UpdatedAt = DateTime.UtcNow;

        _db.Update(user);
        _db.SaveChanges();

        _audit.Log("BLOCK_USER", userId, "User blocked");
        _logger.LogWarning("User {UserId} blocked", userId);
    }

    public void UnblockUser(Guid userId)
    {
        var user = _db.Users.FirstOrDefault(u => u.Id == userId)
                   ?? throw new AppException("User not found", 404);

        user.IsBlocked = false;
        user.UpdatedAt = DateTime.UtcNow;

        _db.Update(user);
        _db.SaveChanges();

        _audit.Log("UNBLOCK_USER", userId, "User unblocked");
    }

    public void DeleteRide(Guid rideId)
    {
        var ride = _db.Rides.FirstOrDefault(r => r.Id == rideId)
                   ?? throw new AppException("Ride not found", 404);

        ride.IsDeleted = true;
        ride.UpdatedAt = DateTime.UtcNow;

        _db.Update(ride);
        _db.SaveChanges();

        _audit.Log("DELETE_RIDE", Guid.Empty, $"Ride {rideId} deleted");
    }
}
