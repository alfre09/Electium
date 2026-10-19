using Electium.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Electium.Persistence.Identity;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    /// <summary>Partido al que pertenece (solo para dirigentes políticos).</summary>
    public int? PartyId { get; set; }

    public Party? Party { get; set; }
}
