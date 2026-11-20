using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIAlquilerVehiculos.Services.Commons;

public interface IService<TCreate,TUpdate,TResponse,TId>
{
    Task<IEnumerable<TResponse>> GetAllAsync();

    Task<TResponse?> GetByIdAsync(TId id);

    Task<TResponse> CreateAsync(TCreate dto);

    Task<TResponse?> UpdateAsync(TId id, TUpdate dto);

    Task<bool> DeleteAsync(TId id);
}