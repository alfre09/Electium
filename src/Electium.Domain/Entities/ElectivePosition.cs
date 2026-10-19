using Electium.Domain.Common;

namespace Electium.Domain.Entities;

/// <summary>Puesto electivo (Presidente, Senador, Alcalde, etc.).</summary>
public class ElectivePosition : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
