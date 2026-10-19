using Electium.Domain.Common;
using Electium.Domain.Enums;

namespace Electium.Domain.Entities;

public class Election : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ElectionStatus Status { get; set; } = ElectionStatus.Draft;

    public DateTime? OpenedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
