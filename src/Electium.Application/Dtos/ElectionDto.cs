using Electium.Domain.Enums;

namespace Electium.Application.Dtos;

public class ElectionDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ElectionStatus Status { get; set; } = ElectionStatus.Draft;
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
