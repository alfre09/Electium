using Electium.Domain.Enums;

namespace Electium.Application.Dtos;

/// <summary>Datos del elector ya validado y de la elección abierta.</summary>
public class VoterContextDto
{
    public int CitizenId { get; set; }
    public string CitizenFullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string IdentityDocument { get; set; } = string.Empty;
    public int ElectionId { get; set; }
    public string ElectionName { get; set; } = string.Empty;
}

public class BallotDto
{
    public int ElectionId { get; set; }
    public string ElectionName { get; set; } = string.Empty;
    public List<BallotPositionDto> Positions { get; set; } = new();
}

public class BallotPositionDto
{
    public int PositionId { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CandidateDto> Candidates { get; set; } = new();
}

public class ElectionResultsDto
{
    public int ElectionId { get; set; }
    public string ElectionName { get; set; } = string.Empty;
    public ElectionStatus Status { get; set; }
    public int EligibleCitizens { get; set; }
    public int VotersCount { get; set; }
    public double ParticipationPercent => EligibleCitizens == 0 ? 0 : VotersCount * 100.0 / EligibleCitizens;
    public List<PositionResultDto> Positions { get; set; } = new();
}

public class PositionResultDto
{
    public int PositionId { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public List<CandidateResultDto> Candidates { get; set; } = new();
}

public class CandidateResultDto
{
    public int CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string PartyName { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
    public int Votes { get; set; }
    public double Percentage { get; set; }
}
