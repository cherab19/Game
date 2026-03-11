using System.Security.Claims;

namespace Game.Backend.Security;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredPlayerId(this ClaimsPrincipal principal)
    {
        var subject = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");

        if (subject is null || !Guid.TryParse(subject, out var playerId))
        {
            throw new UnauthorizedAccessException("A valid player identifier is required.");
        }

        return playerId;
    }
}