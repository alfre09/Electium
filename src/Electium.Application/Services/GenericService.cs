using AutoMapper;
using Electium.Application.Dtos;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Domain.Common;

namespace Electium.Application.Services;

public class GenericService<TDto, TEntity> : IGenericService<TDto, TEntity>
    where TDto : BaseDto
    where TEntity : BaseEntity
{
    protected readonly IGenericRepository<TEntity> Repository;
    protected readonly IMapper Mapper;

    public GenericService(IGenericRepository<TEntity> repository, IMapper mapper)
    {
        Repository = repository;
        Mapper = mapper;
    }

    public virtual async Task<List<TDto>> GetAllAsync()
    {
        var entities = await Repository.GetAllAsync();
        return Mapper.Map<List<TDto>>(entities);
    }

    public virtual async Task<TDto?> GetByIdAsync(int id)
    {
        var entity = await Repository.GetByIdAsync(id);
        return entity is null ? null : Mapper.Map<TDto>(entity);
    }

    public virtual async Task<TDto> AddAsync(TDto dto)
    {
        var entity = Mapper.Map<TEntity>(dto);
        entity.Id = 0;
        await Repository.AddAsync(entity);
        return Mapper.Map<TDto>(entity);
    }

    public virtual async Task UpdateAsync(int id, TDto dto)
    {
        var entity = await Repository.GetByIdAsync(id)
            ?? throw new BusinessException("El registro no existe.");

        Mapper.Map(dto, entity);
        entity.Id = id;
        await Repository.UpdateAsync(entity);
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await Repository.GetByIdAsync(id)
            ?? throw new BusinessException("El registro no existe.");

        await Repository.DeleteAsync(entity);
    }
}
