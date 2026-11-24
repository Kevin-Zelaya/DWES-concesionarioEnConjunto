using APIAlquilerVehiculos.Dtos.Vehiculo;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Services.Commons;
using CSharpFunctionalExtensions;

namespace APIAlquilerVehiculos.Services.Vehiculo;

/// <summary>
/// Contrato de acciones crud de servicio de vehiculos
/// </summary>
public interface IVehiculoService : IService<VehiculoCreateDto, VehiculoUpdateDto, VehiculoResponseDto, string>
{
    /// <summary>
    /// Obtiene un vehículo por su matrícula.
    /// </summary>
    /// <param name="matricula">matrícula del vehículo a buscar</param>
    /// <returns> Devuelve el vehículo si se encuentra, de lo contrario null</returns>
    public Task<Result<VehiculoResponseDto,DomainError>> GetByMatriculaAsync(string matricula,CancellationToken ct = default);

    /// <summary>
    /// Devuelve una paginacion de los vehixulos obtenidos en la base de datos
    /// </summary>
    /// <param name="pageNumber"> Número de páginas totales</param>
    /// <param name="pageSize"> Cantidad de elementos por página</param>
    /// <param name="ct"> Token de cancelacion</param>
    /// <returns> Retorna la página correspondent y una tuple con los datos de pagination </returns>
    public Task<Result<(List<VehiculoResponseDto> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage), DomainError>>
        GetPaginatedVehiculosAsync
        (int pageNumber, int pageSize, CancellationToken ct = default);
}