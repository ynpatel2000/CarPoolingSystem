using Carpooling.API.Extensions;
using Carpooling.Application.Common.Pagination;
using Carpooling.Application.Common.Querying;
using Carpooling.Application.DTOs.Ride;
using Carpooling.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Carpooling.API.Controllers;

[ApiVersion("1.0")]
[Authorize(Roles = "User")]
[ApiController]
[Route("api/v{version:apiVersion}/rides")]
public class RidesController : ControllerBase
{
    private readonly IRideService _service;
    private readonly ILogger<RidesController> _logger;

    public RidesController(
        IRideService service,
        ILogger<RidesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // =====================================================
    // CREATE RIDE (USER AS DRIVER)
    // =====================================================
    // POST: api/v1/rides
    // =====================================================
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Create(
        [FromBody] CreateRideDto dto,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        _logger.LogInformation(
            "Create ride request by UserId={UserId}",
            userId
        );

        _service.CreateRide(userId, dto);

        _logger.LogInformation(
            "Ride created successfully by UserId={UserId}",
            userId
        );

        return Ok(new { message = "Ride created successfully" });
    }

    // =====================================================
    // GET MY RIDES (USER AS DRIVER)
    // =====================================================
    // GET: api/v1/rides/my
    // =====================================================
    [HttpGet("my")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult MyRides(
        [FromQuery] PagedRequest page,
        [FromQuery] SortRequest sort,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (page.PageSize <= 0)
            return BadRequest("PageSize must be greater than zero");

        _logger.LogInformation(
            "My rides requested by UserId={UserId}",
            userId
        );

        var result = _service.GetMyRides(userId, page, sort);

        return Ok(result);
    }

    // =====================================================
    // SEARCH RIDES (PASSENGER)
    // =====================================================
    // GET: api/v1/rides/search
    // =====================================================
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Search(
        [FromQuery] RideSearchDto dto,
        [FromQuery] PagedRequest page,
        [FromQuery] SortRequest sort,
        CancellationToken cancellationToken)
    {
        if (page.PageSize <= 0)
            return BadRequest("PageSize must be greater than zero");

        if (dto.RequiredSeats <= 0)
            return BadRequest("RequiredSeats must be greater than zero");

        _logger.LogInformation(
            "Ride search requested From={From} To={To} Date={Date}",
            dto.FromCity,
            dto.ToCity,
            dto.RideDate
        );

        var result = _service.Search(dto, page, sort);

        return Ok(result);
    }
}