using System.Security.Claims;

namespace Carpooling.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new Exception("User ID claim not found");

        return Guid.Parse(userIdClaim.Value);
    }
}
