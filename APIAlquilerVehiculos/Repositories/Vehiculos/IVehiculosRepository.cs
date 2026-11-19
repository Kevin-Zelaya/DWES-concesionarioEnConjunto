using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using APIAlquilerVehiculos.Models;

namespace APIAlquilerVehiculos.Repositories.Vehiculos;

/// <summary>
/// Define la interfaz para el repositorio de vehículos,
/// proporcionando métodos para realizar operaciones CRUD y consultas específicas relacionadas con los vehículos.
/// </summary>
public interface IVehiculosRepository: ICrudRepository<Vehiculo,string>
{
    /// <summary>
    /// Obtiene un vehículo por su matrícula.
    /// </summary>
    /// <param name="matricula">matrícula del vehículo a buscar</param>
    /// <returns> Devuelve el vehículo si se encuentra, de lo contrario null</returns>
    public Task<Vehiculo?> GetByMatriculaAsync(string matricula);

    /// <summary>
    /// Devuelve una paginacion de los vehixulos obtenidos en la base de datos
    /// </summary>
    /// <param name="pageNumber"> Número de páginas totales</param>
    /// <param name="pageSize"> Cantidad de elementos por página</param>
    /// <param name="ct"> Token de cancelacion</param>
    /// <returns> Retorna la página correspondent y una tuple con los datos de pagination </returns>
    public Task<(List<Vehiculo> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)>
        GetPaginatedVehiculosAsync
        (int pageNumber, int pageSize, CancellationToken ct = default);
}