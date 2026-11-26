using System.Security.Claims;

namespace SoundRevival.WebApi.Extensions;


public static class ClaimsPrincipalExtensions
{

    public static Guid? GetUserIdOrNull(this ClaimsPrincipal user)
    {
        string? value = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var result) ? result : null;

    }

    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        string? value = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var result))
        {
            throw new InvalidOperationException("Authenticated user has no valid NameIdentifier claim");
        }

        return result;
    }
}
