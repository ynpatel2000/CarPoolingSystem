using Carpooling.API.Extensions;
using Carpooling.Application.Common.Pagination;
using Carpooling.Application.DTOs.Booking;
using Carpooling.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Carpooling.API.Controllers;

[ApiVersion("1.0")]
[Authorize(Roles = "User")]
[ApiController]
[Route("api/v{version:apiVersion}/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(
        IBookingService service,
        ILogger<BookingsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // =====================================================
    // BOOK A RIDE (USER AS PASSENGER)
    // =====================================================
    // POST: api/v1/bookings
    // =====================================================
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Book(
        [FromBody] CreateBookingDto dto,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        _logger.LogInformation(
            "Booking request received by UserId={UserId} for RideId={RideId}",
            userId,
            dto.RideId
        );

        _service.BookRide(userId, dto);

        _logger.LogInformation(
            "Booking successful by UserId={UserId} for RideId={RideId}",
            userId,
            dto.RideId
        );

        return Ok(new { message = "Ride booked successfully" });
    }

    // =====================================================
    // GET MY BOOKINGS
    // =====================================================
    // GET: api/v1/bookings/my
    // =====================================================
    [HttpGet("my")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult MyBookings(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (request.PageSize <= 0)
            return BadRequest("PageSize must be greater than zero");

        _logger.LogInformation(
            "My bookings requested by UserId={UserId}",
            userId
        );

        var result = _service.GetMyBookings(userId, request);

        return Ok(result);
    }

    // =====================================================
    // CANCEL BOOKING
    // =====================================================
    // DELETE: api/v1/bookings/{id}
    // =====================================================
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        _logger.LogInformation(
            "Cancel booking request by UserId={UserId} BookingId={BookingId}",
            userId,
            id
        );

        _service.CancelBooking(id, userId);

        _logger.LogInformation(
            "Booking cancelled by UserId={UserId} BookingId={BookingId}",
            userId,
            id
        );

        return Ok(new { message = "Booking cancelled successfully" });
    }
}