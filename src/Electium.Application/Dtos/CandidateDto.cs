namespace Electium.Application.Dtos;

public class CandidateDto : BaseDto
{
    public int ElectionId { get; set; }
    public int PartyId { get; set; }
    public int ElectivePositionId { get; set; }
    public int CitizenId { get; set; }
    public string? PhotoPath { get; set; }
    public bool IsActive { get; set; } = true;

    // Datos de solo lectura para mostrar en pantallas
    public string ElectionName { get; set; } = string.Empty;
    public string PartyName { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public string CitizenFullName { get; set; } = string.Empty;
}
