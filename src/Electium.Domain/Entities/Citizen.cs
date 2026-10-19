using Electium.Domain.Common;

namespace Electium.Domain.Entities;

public class Citizen : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>Documento de identidad guardado solo con dígitos (11).</summary>
    public string IdentityDocument { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<Candidate> Candidacies { get; set; } = new List<Candidate>();

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
