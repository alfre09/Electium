using Electium.Domain.Common;

namespace Electium.Domain.Entities;

public class Candidate : BaseEntity
{
    public int ElectionId { get; set; }
    public Election Election { get; set; } = null!;

    public int PartyId { get; set; }
    public Party Party { get; set; } = null!;

    public int ElectivePositionId { get; set; }
    public ElectivePosition ElectivePosition { get; set; } = null!;

    public int CitizenId { get; set; }
    public Citizen Citizen { get; set; } = null!;

    public string? PhotoPath { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
