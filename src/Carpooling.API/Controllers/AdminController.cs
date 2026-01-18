using Carpooling.Application.Common.Pagination;
using Carpooling.Application.Common.Querying;
using Carpooling.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Carpooling.API.Controllers;

[ApiVersion("1.0")]
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/v{version:apiVersion}/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAdminService adminService,
        ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    // =====================================================
    // GET USERS (ADMIN)
    // =====================================================
    // GET: api/v1/admin/users
    // =====================================================
    [HttpGet("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Users(
        [FromQuery] PagedRequest page,
        [FromQuery] SortRequest sort,
        [FromQuery] FilterRequest filter,
        CancellationToken cancellationToken)
    {
        if (page.PageSize <= 0)
            return BadRequest("PageSize must be greater than zero");

        _logger.LogInformation(
            "Admin requested users list | Page={Page} Size={Size}",
            page.Page,
            page.PageSize
        );

        var result = _adminService.GetUsers(page, sort, filter);

        return Ok(result);
    }

    // =====================================================
    // GET BOOKINGS (ADMIN)
    // =====================================================
    // GET: api/v1/admin/bookings
    // =====================================================
    [HttpGet("bookings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Bookings(
        [FromQuery] PagedRequest page,
        [FromQuery] SortRequest sort,
        CancellationToken cancellationToken)
    {
        if (page.PageSize <= 0)
            return BadRequest("PageSize must be greater than zero");

        _logger.LogInformation(
            "Admin requested bookings list | Page={Page} Size={Size}",
            page.Page,
            page.PageSize
        );

        var result = _adminService.GetBookings(page, sort);

        return Ok(result);
    }

    // =====================================================
    // GET AUDIT LOGS (ADMIN)
    // =====================================================
    // GET: api/v1/admin/audit-logs
    // =====================================================
    [HttpGet("audit-logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult AuditLogs(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        if (request.PageSize <= 0)
            return BadRequest("PageSize must be greater than zero");

        _logger.LogInformation(
            "Admin requested audit logs | Page={Page} Size={Size}",
            request.Page,
            request.PageSize
        );

        var result = _adminService.GetAuditLogs(request);

        return Ok(result);
    }
}
