using Carpooling.API.Extensions;
using Carpooling.Application.DTOs.Ride;
using Carpooling.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Carpooling.API.Controllers;

[Authorize(Roles = "User")]
[ApiController]
[Route("api/rides")]
public class RidesController : ControllerBase
{
    private readonly IRideService _rideService;

    public RidesController(IRideService rideService)
    {
        _rideService = rideService;
    }

    // =====================================================
    // GET RIDE BY ID (DETAIL VIEW)
    // GET: api/rides/{id}
    // =====================================================
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var ride = _rideService.GetById(id);
        return Ok(ride);
    }

    // =====================================================
    // SEARCH RIDES (PASSENGER)
    // GET: api/rides/search
    // =====================================================
    [HttpGet("search")]
    public IActionResult Search([FromQuery] RideSearchDto dto)
    {
        var rides = _rideService.Search(dto);
        return Ok(rides);
    }

    // =====================================================
    // GET MY RIDES (USER AS DRIVER)
    // GET: api/rides/my
    // =====================================================
    [HttpGet("my")]
    public IActionResult GetMyRides()
    {
        var userId = User.GetUserId();
        var rides = _rideService.GetMyRides(userId);
        return Ok(rides);
    }

    // =====================================================
    // CREATE RIDE (USER AS DRIVER)
    // POST: api/rides
    // =====================================================
    [HttpPost]
    public IActionResult Create([FromBody] CreateRideDto dto)
    {
        var userId = User.GetUserId();
        _rideService.CreateRide(userId, dto);
        return Ok(new { message = "Ride created successfully" });
    }

    // =====================================================
    // UPDATE RIDE (ONLY DRIVER)
    // PUT: api/rides/{id}
    // =====================================================
    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] UpdateRideDto dto)
    {
        var userId = User.GetUserId();
        _rideService.UpdateRide(id, userId, dto);
        return Ok(new { message = "Ride updated successfully" });
    }

    // =====================================================
    // DELETE RIDE
    // DRIVER: delete own ride
    // ADMIN: delete any ride
    // DELETE: api/rides/{id}
    // =====================================================
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var userId = User.GetUserId();
        var isAdmin = User.IsInRole("Admin");

        _rideService.DeleteRide(id, userId, isAdmin);
        return Ok(new { message = "Ride deleted successfully" });
    }
}
