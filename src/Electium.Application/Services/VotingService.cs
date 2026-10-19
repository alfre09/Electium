using System.Net;
using AutoMapper;
using Electium.Application.Dtos;
using Electium.Application.Exceptions;
using Electium.Application.Helpers;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;
using Electium.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Electium.Application.Services;

public class VotingService : IVotingService
{
    private static readonly string[] CandidateIncludes = { "Party", "ElectivePosition", "Citizen" };

    private readonly IGenericRepository<Election> _elections;
    private readonly IGenericRepository<Citizen> _citizens;
    private readonly IGenericRepository<Candidate> _candidates;
    private readonly IGenericRepository<Vote> _votes;
    private readonly IGenericRepository<ElectivePosition> _positions;
    private readonly IEmailService _email;
    private readonly IMapper _mapper;
    private readonly ILogger<VotingService> _logger;

    public VotingService(
        IGenericRepository<Election> elections,
        IGenericRepository<Citizen> citizens,
        IGenericRepository<Candidate> candidates,
        IGenericRepository<Vote> votes,
        IGenericRepository<ElectivePosition> positions,
        IEmailService email,
        IMapper mapper,
        ILogger<VotingService> logger)
    {
        _elections = elections;
        _citizens = citizens;
        _candidates = candidates;
        _votes = votes;
        _positions = positions;
        _email = email;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<VoterContextDto> ValidateVoterAsync(string document)
    {
        var election = await GetOpenElectionAsync();

        var normalized = DocumentHelper.Normalize(document);
        if (normalized.Length != DocumentHelper.DocumentLength)
            throw new BusinessException("El documento de identidad debe tener 11 dígitos.");

        var citizen = (await _citizens.FindAsync(c => c.IdentityDocument == normalized)).FirstOrDefault();
        if (citizen is null || !citizen.IsActive)
            throw new BusinessException("El documento no corresponde a un ciudadano habilitado para votar.");

        var electionId = election.Id;
        var citizenId = citizen.Id;
        if (await _votes.AnyAsync(v => v.ElectionId == electionId && v.CitizenId == citizenId))
            throw new BusinessException("Este ciudadano ya ejerció su voto en la elección actual.");

        return new VoterContextDto
        {
            CitizenId = citizen.Id,
            CitizenFullName = $"{citizen.Name} {citizen.LastName}".Trim(),
            Email = citizen.Email,
            IdentityDocument = citizen.IdentityDocument,
            ElectionId = election.Id,
            ElectionName = election.Name
        };
    }

    public async Task<BallotDto> GetBallotAsync(int electionId)
    {
        var election = await _elections.GetByIdAsync(electionId)
            ?? throw new BusinessException("La elección no existe.");

        var candidates = await _candidates.FindAsync(
            c => c.ElectionId == electionId
              && c.IsActive
              && c.Party.IsActive
              && c.ElectivePosition.IsActive,
            CandidateIncludes);

        var ballot = new BallotDto
        {
            ElectionId = election.Id,
            ElectionName = election.Name
        };

        foreach (var group in candidates.GroupBy(c => c.ElectivePosition).OrderBy(g => g.Key.Name))
        {
            ballot.Positions.Add(new BallotPositionDto
            {
                PositionId = group.Key.Id,
                PositionName = group.Key.Name,
                Description = group.Key.Description,
                Candidates = _mapper.Map<List<CandidateDto>>(group.OrderBy(c => c.Party.Name))
            });
        }

        return ballot;
    }

    public async Task CastVotesAsync(int citizenId, int electionId, IDictionary<int, int> selections)
    {
        var election = await GetOpenElectionAsync();
        if (election.Id != electionId)
            throw new BusinessException("La elección ya no está abierta.");

        var citizen = await _citizens.GetByIdAsync(citizenId);
        if (citizen is null || !citizen.IsActive)
            throw new BusinessException("El ciudadano no está habilitado para votar.");

        if (await _votes.AnyAsync(v => v.ElectionId == electionId && v.CitizenId == citizenId))
            throw new BusinessException("Este ciudadano ya ejerció su voto en la elección actual.");

        if (selections.Count == 0)
            throw new BusinessException("Debe seleccionar al menos un candidato.");

        var validCandidates = await _candidates.FindAsync(c => c.ElectionId == electionId && c.IsActive);
        var votes = new List<Vote>();

        foreach (var (positionId, candidateId) in selections)
        {
            var candidate = validCandidates.FirstOrDefault(c => c.Id == candidateId && c.ElectivePositionId == positionId)
                ?? throw new BusinessException("Una de las selecciones no es válida para esta elección.");

            votes.Add(new Vote
            {
                ElectionId = electionId,
                CitizenId = citizenId,
                ElectivePositionId = candidate.ElectivePositionId,
                CandidateId = candidate.Id
            });
        }

        // Un solo SaveChanges: los votos se guardan todos o ninguno.
        await _votes.AddRangeAsync(votes);

        await SendConfirmationAsync(citizen, election);
    }

    public async Task<ElectionResultsDto> GetResultsAsync(int electionId, int? partyId = null)
    {
        var election = await _elections.GetByIdAsync(electionId)
            ?? throw new BusinessException("La elección no existe.");

        var candidates = await _candidates.FindAsync(c => c.ElectionId == electionId, CandidateIncludes);
        var votes = await _votes.FindAsync(v => v.ElectionId == electionId);

        var results = new ElectionResultsDto
        {
            ElectionId = election.Id,
            ElectionName = election.Name,
            Status = election.Status,
            EligibleCitizens = await _citizens.CountAsync(c => c.IsActive),
            VotersCount = votes.Select(v => v.CitizenId).Distinct().Count()
        };

        foreach (var group in candidates.GroupBy(c => c.ElectivePosition).OrderBy(g => g.Key.Name))
        {
            // El total de la fila se calcula con todos los candidatos del puesto,
            // aunque el dirigente solo vea los de su partido.
            var positionVotes = votes.Count(v => v.ElectivePositionId == group.Key.Id);

            var rows = group
                .Where(c => partyId == null || c.PartyId == partyId)
                .Select(c =>
                {
                    var count = votes.Count(v => v.CandidateId == c.Id);
                    return new CandidateResultDto
                    {
                        CandidateId = c.Id,
                        CandidateName = $"{c.Citizen.Name} {c.Citizen.LastName}".Trim(),
                        PartyName = c.Party.Name,
                        PhotoPath = c.PhotoPath,
                        Votes = count,
                        Percentage = positionVotes == 0 ? 0 : count * 100.0 / positionVotes
                    };
                })
                .OrderByDescending(r => r.Votes)
                .ThenBy(r => r.CandidateName)
                .ToList();

            if (rows.Count == 0)
                continue;

            results.Positions.Add(new PositionResultDto
            {
                PositionId = group.Key.Id,
                PositionName = group.Key.Name,
                TotalVotes = positionVotes,
                Candidates = rows
            });
        }

        return results;
    }

    private async Task<Election> GetOpenElectionAsync()
    {
        var open = await _elections.FindAsync(e => e.Status == ElectionStatus.Open);
        return open.FirstOrDefault()
            ?? throw new BusinessException("No hay un proceso electoral abierto en este momento.");
    }

    private async Task SendConfirmationAsync(Citizen citizen, Election election)
    {
        try
        {
            var name = WebUtility.HtmlEncode(citizen.Name);
            var electionName = WebUtility.HtmlEncode(election.Name);

            var body = $@"
<div style=""font-family:Segoe UI,Arial,sans-serif;max-width:520px;margin:auto;color:#0f172a"">
  <h2 style=""color:#172554"">Voto registrado</h2>
  <p>Hola {name},</p>
  <p>Tu participación en <strong>{electionName}</strong> fue registrada correctamente el
     {DateTime.Now:dd/MM/yyyy} a las {DateTime.Now:hh:mm tt}.</p>
  <p>Por seguridad, este mensaje no indica por quién votaste: el voto es secreto.</p>
  <p style=""color:#64748b;font-size:12px"">Electium · Plataforma de votación electrónica</p>
</div>";

            await _email.SendAsync(citizen.Email, "Confirmación de voto - Electium", body);
        }
        catch (Exception ex)
        {
            // El voto ya quedó guardado; un fallo de correo no debe invalidarlo.
            _logger.LogWarning(ex, "No se pudo enviar el correo de confirmación al ciudadano {CitizenId}.", citizen.Id);
        }
    }
}
