using System.Collections.Generic;
using System.Threading.Tasks;
using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

namespace APIAlquilerVehiculos.Services.Commons;

public interface IService<TCreate,TUpdate,TResponse,TId>
{
    Task<Result<List<TResponse>,DomainError>> GetAllAsync(CancellationToken ct = default);

    Task<Result<TResponse,DomainError>> GetByIdAsync(TId id,CancellationToken ct = default);

    Task<Result<TResponse,DomainError>> CreateAsync(TCreate dto,CancellationToken ct = default);

    Task<Result<TResponse,DomainError>> UpdateAsync(TId id, TUpdate dto,CancellationToken ct = default);

    Task<UnitResult<DomainError>> DeleteAsync(TId id,CancellationToken ct = default);
}