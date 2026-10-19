namespace Electium.Application.Dtos;

public class CitizenDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string IdentityDocument { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public string FullName => $"{Name} {LastName}".Trim();
}
