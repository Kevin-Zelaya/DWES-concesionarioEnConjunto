using APIAlquilerVehiculos.DTOs.Marcas;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Extensions;
using CSharpFunctionalExtensions;

namespace APIAlquilerVehiculos.Services.Marcas;

public interface IMarcaService
{
    Task<Result<List<MarcaDto>, DomainError>> GetAllAsync();

    Task<Result<PaginatedResouce<MarcaDto>, DomainError>>
        GetPaginatedMarcasAsync(
            int pageNumber,
            int pageSize);

    Task<Result<MarcaDto, DomainError>> GetByIdAsync(long id);

    Task<Result<MarcaDto, DomainError>> GetByNombreAsync(string nombre);

    Task<Result<MarcaDto, DomainError>> CreateAsync(CreateMarcaDto dto);

    Task<Result<MarcaDto, DomainError>> UpdateAsync(
        long id,
        UpdateMarcaDto dto);

    Task<Result<bool, DomainError>> DeleteAsync(long id);
}