
using APIAlquilerVehiculos.DTOs.Marcas;
using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

namespace APIAlquilerVehiculos.Services.Marcas;

public interface IMarcaService
{
    Task<Result<List<MarcaDto>, DomainError>> GetAllAsync();

    Task<Result<(List<MarcaDto> items, int count, int totalPages,
        bool HasPreviousPage, bool HasNextPage), DomainError>>
        GetPaginatedMarcasAsync(
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);

    Task<Result<MarcaDto, DomainError>> GetByIdAsync(long id);

    Task<Result<MarcaDto, DomainError>> GetByNombreAsync(string nombre);

    Task<Result<MarcaDto, DomainError>> CreateAsync(CreateMarcaDto dto);

    Task<Result<MarcaDto, DomainError>> UpdateAsync(
        long id,
        UpdateMarcaDto dto);

    Task<Result<bool, DomainError>> DeleteAsync(long id);
}
