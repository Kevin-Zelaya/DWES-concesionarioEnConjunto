using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

/// <summary>
/// Define las operaciones disponibles para gestionar los extras.
/// </summary>
public interface IExtraService
{
    Task<Result<ExtraResponseDto, DomainError>> CreateAsync(ExtraCreateRequestDto request, CancellationToken ct = default);

    Task<Result<ExtraResponseDto, DomainError>> GetByIdAsync(long id, CancellationToken ct = default);

    Task<Result<List<ExtraResponseDto>, DomainError>> GetAllAsync(CancellationToken ct = default);

    Task<Result<ExtraResponseDto, DomainError>> UpdateAsync(long id, ExtraUpdateRequestDto request, CancellationToken ct = default);

    Task<Result<bool, DomainError>> DeleteAsync(long id, CancellationToken ct = default);
}