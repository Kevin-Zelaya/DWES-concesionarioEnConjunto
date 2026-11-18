using APIAlquilerVehiculos.Models;

namespace APIAlquilerVehiculos.Repositories.Modelos;

/// <summary>
/// Define la interfaz para el repositorio de modelos,
/// proporcionando métodos para realizar operaciones CRUD y consultas específicas relacionadas con los modelos.
/// </summary>
public interface IModeloRepository : ICrudRepository<Modelo, int>
{
    /// <summary>
    /// Devuelve una paginacion de los modelos obtenidos en la base de datos
    /// </summary>
    /// <param name="pageNumber"> Número de páginas totales</param>
    /// <param name="pageSize"> Cantidad de elementos por página</param>
    /// <param name="ct"> Token de cancelacion</param>
    /// <returns> Retorna la página correspondent y una tupla con los datos de pagination </returns>
    public Task<(List<Modelo> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)>
        GetPaginatedModeloAsync
        (int pageNumber, int pageSize, CancellationToken ct = default);
}