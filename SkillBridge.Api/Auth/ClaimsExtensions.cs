using System.Security.Claims;

namespace SkillBridge.Api.Auth;

public static class ClaimsExtensions
{
    public static bool TryGetUserId(this ClaimsPrincipal principal, out Guid userId)
    {
        userId = default;
        var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return sub is not null && Guid.TryParse(sub, out userId);
    }
}
