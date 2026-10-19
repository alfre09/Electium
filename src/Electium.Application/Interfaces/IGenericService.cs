using Electium.Application.Dtos;
using Electium.Domain.Common;

namespace Electium.Application.Interfaces;

public interface IGenericService<TDto, TEntity>
    where TDto : BaseDto
    where TEntity : BaseEntity
{
    Task<List<TDto>> GetAllAsync();

    Task<TDto?> GetByIdAsync(int id);

    Task<TDto> AddAsync(TDto dto);

    Task UpdateAsync(int id, TDto dto);

    Task DeleteAsync(int id);
}
