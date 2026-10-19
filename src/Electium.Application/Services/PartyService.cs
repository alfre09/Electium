using AutoMapper;
using Electium.Application.Dtos;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;

namespace Electium.Application.Services;

public class PartyService : GenericService<PartyDto, Party>, IPartyService
{
    public PartyService(IGenericRepository<Party> repository, IMapper mapper)
        : base(repository, mapper)
    {
    }

    public override async Task<List<PartyDto>> GetAllAsync()
    {
        var list = await base.GetAllAsync();
        return list.OrderBy(p => p.Name).ToList();
    }

    public override async Task<PartyDto> AddAsync(PartyDto dto)
    {
        await ValidateAsync(dto, 0);
        return await base.AddAsync(dto);
    }

    public override async Task UpdateAsync(int id, PartyDto dto)
    {
        await ValidateAsync(dto, id);
        await base.UpdateAsync(id, dto);
    }

    private async Task ValidateAsync(PartyDto dto, int currentId)
    {
        dto.Name = dto.Name.Trim();
        var name = dto.Name.ToLower();

        if (await Repository.AnyAsync(p => p.Name.ToLower() == name && p.Id != currentId))
            throw new BusinessException("Ya existe un partido con ese nombre.");
    }
}
