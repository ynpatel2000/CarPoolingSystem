using Carpooling.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Carpooling.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAppDbContext _db;

    public AdminController(IAppDbContext db)
    {
        _db = db;
    }

    // =====================================================
    // DASHBOARD STATS
    // GET: api/admin/stats
    // =====================================================
    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        return Ok(new
        {
            Users = _db.Users.Count(),
            Rides = _db.Rides.Count(),
            Bookings = _db.Bookings.Count()
        });
    }

    // =====================================================
    // GET ALL USERS
    // GET: api/admin/users
    // =====================================================
    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        return Ok(_db.Users.ToList());
    }

    // =====================================================
    // BLOCK / UNBLOCK USER
    // PUT: api/admin/users/{id}/block
    // =====================================================
    [HttpPut("users/{id:guid}/block")]
    public IActionResult BlockUser(Guid id)
    {
        var user = _db.Users.FirstOrDefault(x => x.Id == id);
        if (user == null)
            return NotFound("User not found");

        user.IsBlocked = true;
        _db.SaveChanges();

        return Ok("User blocked");
    }

    // =====================================================
    // UNBLOCK USER
    // PUT: api/admin/users/{id}/unblock
    // =====================================================
    [HttpPut("users/{id:guid}/unblock")]
    public IActionResult UnblockUser(Guid id)
    {
        var user = _db.Users.FirstOrDefault(x => x.Id == id);
        if (user == null)
            return NotFound("User not found");

        user.IsBlocked = false;
        _db.SaveChanges();

        return Ok("User unblocked");
    }

    // =====================================================
    // DELETE ANY RIDE
    // DELETE: api/admin/rides/{id}
    // =====================================================
    [HttpDelete("rides/{id:guid}")]
    public IActionResult DeleteRide(Guid id)
    {
        var ride = _db.Rides.FirstOrDefault(r => r.Id == id);
        if (ride == null)
            return NotFound("Ride not found");

        _db.Remove(ride);
        _db.SaveChanges();

        return Ok("Ride deleted by admin");
    }
}
