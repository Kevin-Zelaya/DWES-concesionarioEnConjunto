using APIAlquilerVehiculos.Models;

namespace APIAlquilerVehiculos.Repositories.Marcas;

/// <summary>
/// Define la interfaz para el repositorio de marcas,
/// proporcionando métodos para realizar operaciones CRUD
/// y consultas específicas relacionadas con las marcas.
/// </summary>
public interface IMarcaRepository : ICrudRepository<Marca, long>
{
    /// <summary>
    /// Devuelve una paginación de las marcas obtenidas
    /// en la base de datos.
    /// </summary>
    /// <param name="pageNumber">Número de página solicitada.</param>
    /// <param name="pageSize">Cantidad de elementos por página.</param>
    /// <returns>
    /// Retorna la página correspondiente y una tupla
    /// con los datos de paginación.
    /// </returns>
    Task<(List<Marca> items, int count, int totalPages,
        bool HasPreviousPage, bool HasNextPage)>
        GetPaginatedMarcasAsync(
            int pageNumber,
            int pageSize);

    /// <summary>
    /// Obtiene una marca por su nombre.
    /// </summary>
    Task<Marca?> GetByNombreAsync(string nombre);
}