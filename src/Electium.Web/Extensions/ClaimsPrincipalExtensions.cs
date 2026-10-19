using System.Security.Claims;
using Electium.Application.Constants;

namespace Electium.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Partido del dirigente autenticado, o null si no es dirigente.</summary>
    public static int? GetPartyId(this ClaimsPrincipal user)
    {
        if (!user.IsInRole(AppRoles.Leader))
            return null;

        var value = user.FindFirstValue(AppClaims.PartyId);
        return int.TryParse(value, out var id) ? id : null;
    }
}
