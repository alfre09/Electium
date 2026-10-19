using System.ComponentModel.DataAnnotations;

namespace Electium.Application.ViewModels;

public class CitizenViewModel : BaseViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, ErrorMessage = "Máximo 80 caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(80, ErrorMessage = "Máximo 80 caracteres.")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
    [StringLength(120, ErrorMessage = "Máximo 120 caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
    [RegularExpression(@"^\d{3}-?\d{7}-?\d$", ErrorMessage = "Formato válido: 000-0000000-0 (11 dígitos).")]
    [Display(Name = "Documento de identidad")]
    public string IdentityDocument { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;
}
