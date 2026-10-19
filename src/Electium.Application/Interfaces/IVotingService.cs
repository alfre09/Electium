using Electium.Application.Dtos;

namespace Electium.Application.Interfaces;

public interface IVotingService
{
    /// <summary>
    /// Valida que exista una elección abierta y que el documento pertenezca a un
    /// ciudadano activo que todavía no ha votado. Lanza BusinessException si no.
    /// </summary>
    Task<VoterContextDto> ValidateVoterAsync(string document);

    Task<BallotDto> GetBallotAsync(int electionId);

    /// <param name="selections">PuestoId -> CandidatoId</param>
    Task CastVotesAsync(int citizenId, int electionId, IDictionary<int, int> selections);

    Task<ElectionResultsDto> GetResultsAsync(int electionId, int? partyId = null);
}
