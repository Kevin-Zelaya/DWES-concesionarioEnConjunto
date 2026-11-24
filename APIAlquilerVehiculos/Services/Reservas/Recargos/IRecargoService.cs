using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

/// <summary>
/// Define las operaciones disponibles para gestionar los recargos.
/// </summary>
public interface IRecargoService
{
    Task<Result<RecargoResponseDto, DomainError>> CreateAsync(RecargoCreateRequestDto request, CancellationToken ct = default);

    Task<Result<RecargoResponseDto, DomainError>> GetByIdAsync(long id, CancellationToken ct = default);

    Task<Result<List<RecargoResponseDto>, DomainError>> GetAllAsync(CancellationToken ct = default);

    Task<Result<RecargoResponseDto, DomainError>> UpdateAsync(long id, RecargoUpdateRequestDto request, CancellationToken ct = default);

    Task<Result<bool, DomainError>> DeleteAsync(long id, CancellationToken ct = default);
}