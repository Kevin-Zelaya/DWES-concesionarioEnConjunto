using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

/// <summary>
/// Define las operaciones disponibles para gestionar las pólizas.
/// </summary>
public interface IPolizaService
{
    Task<Result<PolizaResponseDto, DomainError>> CreateAsync(PolizaCreateRequestDto request, CancellationToken ct = default);

    Task<Result<PolizaResponseDto, DomainError>> GetByIdAsync(long id, CancellationToken ct = default);

    Task<Result<List<PolizaResponseDto>, DomainError>> GetAllAsync(CancellationToken ct = default);

    Task<Result<PolizaResponseDto, DomainError>> UpdateAsync(long id, PolizaUpdateRequestDto request, CancellationToken ct = default);

    Task<Result<bool, DomainError>> DeleteAsync(long id, CancellationToken ct = default);
}