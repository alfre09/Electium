using Electium.Domain.Common;

namespace Electium.Domain.Entities;

/// <summary>
/// Un voto por ciudadano, elección y puesto. El voto es secreto: nunca se
/// expone a nivel de interfaz qué candidato eligió cada ciudadano.
/// </summary>
public class Vote : BaseEntity
{
    public int ElectionId { get; set; }
    public Election Election { get; set; } = null!;

    public int CitizenId { get; set; }
    public Citizen Citizen { get; set; } = null!;

    public int ElectivePositionId { get; set; }
    public ElectivePosition ElectivePosition { get; set; } = null!;

    public int CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
}
