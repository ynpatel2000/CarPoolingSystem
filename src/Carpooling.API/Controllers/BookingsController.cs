using Carpooling.API.Extensions;
using Carpooling.Application.DTOs.Booking;
using Carpooling.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Carpooling.API.Controllers;

[Authorize(Roles = "User")]
[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // =====================================================
    // BOOK A RIDE (USER AS PASSENGER)
    // POST: api/bookings
    // =====================================================
    [HttpPost]
    public IActionResult BookRide([FromBody] CreateBookingDto dto)
    {
        var userId = User.GetUserId();
        _bookingService.BookRide(userId, dto);

        return Ok(new { message = "Ride booked successfully" });
    }

    // =====================================================
    // GET MY BOOKINGS (PASSENGER VIEW)
    // GET: api/bookings/my
    // =====================================================
    [HttpGet("my")]
    public IActionResult GetMyBookings()
    {
        var userId = User.GetUserId();
        var bookings = _bookingService.GetMyBookings(userId);

        return Ok(bookings);
    }

    // =====================================================
    // CANCEL BOOKING (PASSENGER)
    // DELETE: api/bookings/{id}
    // =====================================================
    [HttpDelete("{id:guid}")]
    public IActionResult CancelBooking(Guid id)
    {
        var userId = User.GetUserId();
        _bookingService.CancelBooking(id, userId);

        return Ok(new { message = "Booking cancelled successfully" });
    }
}
