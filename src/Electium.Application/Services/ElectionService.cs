using AutoMapper;
using Electium.Application.Dtos;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;
using Electium.Domain.Enums;

namespace Electium.Application.Services;

public class ElectionService : GenericService<ElectionDto, Election>, IElectionService
{
    private readonly IGenericRepository<Candidate> _candidates;

    public ElectionService(
        IGenericRepository<Election> repository,
        IGenericRepository<Candidate> candidates,
        IMapper mapper) : base(repository, mapper)
    {
        _candidates = candidates;
    }

    public override async Task<List<ElectionDto>> GetAllAsync()
    {
        var list = await base.GetAllAsync();
        return list.OrderByDescending(e => e.Id).ToList();
    }

    public override async Task<ElectionDto> AddAsync(ElectionDto dto)
    {
        dto.Status = ElectionStatus.Draft;
        dto.OpenedAt = null;
        dto.ClosedAt = null;
        dto.CreatedAt = DateTime.UtcNow;
        dto.Name = dto.Name.Trim();
        return await base.AddAsync(dto);
    }

    /// <summary>Solo se editan nombre y descripción; el estado cambia con Abrir/Cerrar.</summary>
    public override async Task UpdateAsync(int id, ElectionDto dto)
    {
        var election = await Repository.GetByIdAsync(id)
            ?? throw new BusinessException("La elección no existe.");

        if (election.Status == ElectionStatus.Closed)
            throw new BusinessException("Una elección cerrada no se puede modificar.");

        election.Name = dto.Name.Trim();
        election.Description = dto.Description;
        await Repository.UpdateAsync(election);
    }

    public override async Task DeleteAsync(int id)
    {
        var election = await Repository.GetByIdAsync(id)
            ?? throw new BusinessException("La elección no existe.");

        if (election.Status != ElectionStatus.Draft)
            throw new BusinessException("Solo se pueden eliminar elecciones en borrador.");

        if (await _candidates.AnyAsync(c => c.ElectionId == id))
            throw new BusinessException("La elección tiene candidatos registrados. Elimínelos primero.");

        await Repository.DeleteAsync(election);
    }

    public async Task<ElectionDto?> GetOpenAsync()
    {
        var open = await Repository.FindAsync(e => e.Status == ElectionStatus.Open);
        var election = open.FirstOrDefault();
        return election is null ? null : Mapper.Map<ElectionDto>(election);
    }

    public async Task OpenAsync(int id)
    {
        var election = await Repository.GetByIdAsync(id)
            ?? throw new BusinessException("La elección no existe.");

        if (election.Status == ElectionStatus.Open)
            throw new BusinessException("La elección ya está abierta.");

        if (election.Status == ElectionStatus.Closed)
            throw new BusinessException("Una elección cerrada no puede reabrirse.");

        if (await Repository.AnyAsync(e => e.Status == ElectionStatus.Open && e.Id != id))
            throw new BusinessException("Ya hay otra elección abierta. Ciérrela antes de abrir esta.");

        var activeCandidates = await _candidates.CountAsync(c => c.ElectionId == id && c.IsActive);
        if (activeCandidates == 0)
            throw new BusinessException("No se puede abrir una elección sin candidatos activos.");

        election.Status = ElectionStatus.Open;
        election.OpenedAt = DateTime.UtcNow;
        await Repository.UpdateAsync(election);
    }

    public async Task CloseAsync(int id)
    {
        var election = await Repository.GetByIdAsync(id)
            ?? throw new BusinessException("La elección no existe.");

        if (election.Status != ElectionStatus.Open)
            throw new BusinessException("Solo se puede cerrar una elección abierta.");

        election.Status = ElectionStatus.Closed;
        election.ClosedAt = DateTime.UtcNow;
        await Repository.UpdateAsync(election);
    }
}
