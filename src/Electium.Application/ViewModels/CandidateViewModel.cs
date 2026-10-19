using System.ComponentModel.DataAnnotations;

namespace Electium.Application.ViewModels;

public class CandidateViewModel : BaseViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una elección.")]
    [Display(Name = "Elección")]
    public int ElectionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un partido.")]
    [Display(Name = "Partido")]
    public int PartyId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un puesto electivo.")]
    [Display(Name = "Puesto electivo")]
    public int ElectivePositionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un ciudadano.")]
    [Display(Name = "Ciudadano")]
    public int CitizenId { get; set; }

    public string? PhotoPath { get; set; }

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;

    // Listas para los desplegables (no se enlazan desde el formulario)
    public List<LookupItem> Elections { get; set; } = new();
    public List<LookupItem> Parties { get; set; } = new();
    public List<LookupItem> Positions { get; set; } = new();
    public List<LookupItem> Citizens { get; set; } = new();

    /// <summary>True cuando el usuario es dirigente: el partido queda fijo.</summary>
    public bool PartyLocked { get; set; }
}
