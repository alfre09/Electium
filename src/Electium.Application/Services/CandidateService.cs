using AutoMapper;
using Electium.Application.Dtos;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;
using Electium.Domain.Enums;

namespace Electium.Application.Services;

public class CandidateService : GenericService<CandidateDto, Candidate>, ICandidateService
{
    private static readonly string[] Includes = { "Election", "Party", "ElectivePosition", "Citizen" };

    private readonly IGenericRepository<Election> _elections;
    private readonly IGenericRepository<Party> _parties;
    private readonly IGenericRepository<ElectivePosition> _positions;
    private readonly IGenericRepository<Citizen> _citizens;

    public CandidateService(
        IGenericRepository<Candidate> repository,
        IGenericRepository<Election> elections,
        IGenericRepository<Party> parties,
        IGenericRepository<ElectivePosition> positions,
        IGenericRepository<Citizen> citizens,
        IMapper mapper) : base(repository, mapper)
    {
        _elections = elections;
        _parties = parties;
        _positions = positions;
        _citizens = citizens;
    }

    public override async Task<List<CandidateDto>> GetAllAsync()
        => await GetFilteredAsync();

    public async Task<List<CandidateDto>> GetFilteredAsync(int? electionId = null, int? partyId = null)
    {
        var list = await Repository.FindAsync(
            c => (electionId == null || c.ElectionId == electionId)
              && (partyId == null || c.PartyId == partyId),
            Includes);

        return Mapper.Map<List<CandidateDto>>(
            list.OrderBy(c => c.ElectivePosition.Name).ThenBy(c => c.Party.Name));
    }

    public override async Task<CandidateDto?> GetByIdAsync(int id)
    {
        var entity = await Repository.GetByIdAsync(id, Includes);
        return entity is null ? null : Mapper.Map<CandidateDto>(entity);
    }

    public override async Task<CandidateDto> AddAsync(CandidateDto dto)
    {
        await ValidateAsync(dto, 0);
        return await base.AddAsync(dto);
    }

    public override async Task UpdateAsync(int id, CandidateDto dto)
    {
        await ValidateAsync(dto, id);
        await base.UpdateAsync(id, dto);
    }

    public override async Task DeleteAsync(int id)
    {
        var candidate = await Repository.GetByIdAsync(id, "Election")
            ?? throw new BusinessException("El candidato no existe.");

        if (candidate.Election.Status != ElectionStatus.Draft)
            throw new BusinessException("No se puede eliminar un candidato de una elección abierta o cerrada.");

        await Repository.DeleteAsync(candidate);
    }

    private async Task ValidateAsync(CandidateDto dto, int currentId)
    {
        var election = await _elections.GetByIdAsync(dto.ElectionId)
            ?? throw new BusinessException("La elección seleccionada no existe.");

        if (election.Status != ElectionStatus.Draft)
            throw new BusinessException("Solo se pueden registrar o modificar candidatos en elecciones en borrador.");

        var citizen = await _citizens.GetByIdAsync(dto.CitizenId)
            ?? throw new BusinessException("El ciudadano seleccionado no existe.");

        if (!citizen.IsActive)
            throw new BusinessException("El ciudadano seleccionado está inactivo.");

        var party = await _parties.GetByIdAsync(dto.PartyId)
            ?? throw new BusinessException("El partido seleccionado no existe.");

        if (!party.IsActive)
            throw new BusinessException("El partido seleccionado está inactivo.");

        var position = await _positions.GetByIdAsync(dto.ElectivePositionId)
            ?? throw new BusinessException("El puesto electivo seleccionado no existe.");

        if (!position.IsActive)
            throw new BusinessException("El puesto electivo seleccionado está inactivo.");

        if (await Repository.AnyAsync(c => c.ElectionId == dto.ElectionId
                                        && c.CitizenId == dto.CitizenId
                                        && c.Id != currentId))
            throw new BusinessException("Ese ciudadano ya es candidato en esta elección.");

        if (await Repository.AnyAsync(c => c.ElectionId == dto.ElectionId
                                        && c.PartyId == dto.PartyId
                                        && c.ElectivePositionId == dto.ElectivePositionId
                                        && c.Id != currentId))
            throw new BusinessException("Ese partido ya tiene un candidato para ese puesto en esta elección.");
    }
}
