using AutoMapper;
using Electium.Application.Dtos;
using Electium.Application.Exceptions;
using Electium.Application.Helpers;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;

namespace Electium.Application.Services;

public class CitizenService : GenericService<CitizenDto, Citizen>, ICitizenService
{
    public CitizenService(IGenericRepository<Citizen> repository, IMapper mapper)
        : base(repository, mapper)
    {
    }

    public override async Task<List<CitizenDto>> GetAllAsync()
    {
        var list = await base.GetAllAsync();
        return list.OrderBy(c => c.LastName).ThenBy(c => c.Name).ToList();
    }

    public override async Task<CitizenDto> AddAsync(CitizenDto dto)
    {
        await PrepareAndValidateAsync(dto, 0);
        return await base.AddAsync(dto);
    }

    public override async Task UpdateAsync(int id, CitizenDto dto)
    {
        await PrepareAndValidateAsync(dto, id);
        await base.UpdateAsync(id, dto);
    }

    public async Task<CitizenDto?> GetByDocumentAsync(string document)
    {
        var normalized = DocumentHelper.Normalize(document);
        var matches = await Repository.FindAsync(c => c.IdentityDocument == normalized);
        var citizen = matches.FirstOrDefault();
        return citizen is null ? null : Mapper.Map<CitizenDto>(citizen);
    }

    private async Task PrepareAndValidateAsync(CitizenDto dto, int currentId)
    {
        dto.Name = dto.Name.Trim();
        dto.LastName = dto.LastName.Trim();
        dto.Email = dto.Email.Trim().ToLowerInvariant();
        dto.IdentityDocument = DocumentHelper.Normalize(dto.IdentityDocument);

        if (dto.IdentityDocument.Length != DocumentHelper.DocumentLength)
            throw new BusinessException("El documento de identidad debe tener 11 dígitos.");

        var document = dto.IdentityDocument;
        if (await Repository.AnyAsync(c => c.IdentityDocument == document && c.Id != currentId))
            throw new BusinessException("Ya existe un ciudadano con ese documento de identidad.");
    }
}
