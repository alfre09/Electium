using System.ComponentModel.DataAnnotations;
using Electium.Application.Dtos;
using Electium.Domain.Enums;

namespace Electium.Web.ViewModels;

public class VoterLoginViewModel
{
    [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
    [RegularExpression(@"^\d{3}-?\d{7}-?\d$", ErrorMessage = "Formato válido: 000-0000000-0 (11 dígitos).")]
    [Display(Name = "Documento de identidad")]
    public string IdentityDocument { get; set; } = string.Empty;

    [Display(Name = "Foto del documento")]
    public IFormFile? IdentityImage { get; set; }

    public string? ElectionName { get; set; }

    public bool OcrEnabled { get; set; }
}

public class DashboardViewModel
{
    public bool IsLeader { get; set; }
    public string? PartyName { get; set; }
    public int Citizens { get; set; }
    public int Parties { get; set; }
    public int Positions { get; set; }
    public int Elections { get; set; }
    public int Candidates { get; set; }
    public ElectionDto? OpenElection { get; set; }
}

public class ResultsViewModel
{
    public List<ElectionDto> Elections { get; set; } = new();
    public ElectionResultsDto? Results { get; set; }
    public int? SelectedElectionId { get; set; }
    public bool IsLeader { get; set; }
    public string? PartyName { get; set; }
}

public class ElectionSummaryViewModel
{
    public string Name { get; set; } = string.Empty;
    public ElectionStatus Status { get; set; }
    public int PartyCount { get; set; }
    public int CandidateCount { get; set; }
    public int TotalVotes { get; set; }
}

public class HomeViewModel
{
    public bool IsAdmin { get; set; }
    public bool IsLeader { get; set; }
    public bool IsStaff => IsAdmin || IsLeader;

    public bool HasActiveElection { get; set; }
    public string? ActiveElectionName { get; set; }

    public List<int> ElectionYears { get; set; } = new();
    public int? SelectedYear { get; set; }
    public List<ElectionSummaryViewModel> Summaries { get; set; } = new();
}
