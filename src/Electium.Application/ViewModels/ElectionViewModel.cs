using System.ComponentModel.DataAnnotations;
using Electium.Domain.Enums;

namespace Electium.Application.ViewModels;

public class ElectionViewModel : BaseViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "Máximo 120 caracteres.")]
    [Display(Name = "Nombre de la elección")]
    public string Name { get; set; } = string.Empty;

    [StringLength(400, ErrorMessage = "Máximo 400 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    // Solo lectura: se cambian con las acciones Abrir / Cerrar.
    public ElectionStatus Status { get; set; } = ElectionStatus.Draft;
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}
