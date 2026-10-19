namespace Electium.Application.Dtos;

public class LeaderDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? PartyId { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
