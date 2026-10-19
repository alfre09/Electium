using Electium.Application.Dtos;
using Electium.Domain.Entities;

namespace Electium.Application.Interfaces;

public interface ICitizenService : IGenericService<CitizenDto, Citizen>
{
    Task<CitizenDto?> GetByDocumentAsync(string document);
}

public interface IPartyService : IGenericService<PartyDto, Party>
{
}

public interface IElectivePositionService : IGenericService<ElectivePositionDto, ElectivePosition>
{
}

public interface IElectionService : IGenericService<ElectionDto, Election>
{
    Task<ElectionDto?> GetOpenAsync();

    Task OpenAsync(int id);

    Task CloseAsync(int id);
}

public interface ICandidateService : IGenericService<CandidateDto, Candidate>
{
    Task<List<CandidateDto>> GetFilteredAsync(int? electionId = null, int? partyId = null);
}
