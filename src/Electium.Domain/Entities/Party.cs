using Electium.Domain.Common;

namespace Electium.Domain.Entities;

public class Party : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
}
