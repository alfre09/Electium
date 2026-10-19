using System.ComponentModel.DataAnnotations;

namespace Electium.Application.ViewModels;

public class ElectivePositionViewModel : BaseViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    [Display(Name = "Puesto electivo")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Máximo 300 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;
}
