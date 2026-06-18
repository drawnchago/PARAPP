using System.Security.Claims;

namespace PsilmtyApi.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static uint GetUserId(this ClaimsPrincipal user) =>
        uint.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new UnauthorizedAccessException("The authenticated user identifier is missing.");

    public static uint GetParishId(this ClaimsPrincipal user) =>
        uint.TryParse(user.FindFirstValue("parish_id"), out var id) ? id : 0;

    public static bool IsSuperAdmin(this ClaimsPrincipal user) =>
        user.IsInRole(Dictionaries.RoleDictionary.SuperAdmin);
}
