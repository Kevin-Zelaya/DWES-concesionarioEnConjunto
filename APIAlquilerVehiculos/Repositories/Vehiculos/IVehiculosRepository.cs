using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models.Enums;

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
    public Task<Vehiculo?> GetByMatriculaAsync(string matricula,  CancellationToken ct = default);

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

    /// <summary>
    /// Busca todos los vehículos de una categoria
    /// </summary>
    /// <param name="categoria"> Categoria de vehículos</param>
    /// <param name="ct">Token de cancelacion</param>
    /// <returns></returns>
    Task<List<Vehiculo>> GetByCategoriaAsync(CategoriaEnum categoria, CancellationToken ct = default);

    /// <summary>
    /// Bloquea el acceso al vehículo para que nada pueda acceder a este
    /// </summary>
    /// <param name="vehiculoId"> vehiculo a bloquear</param>
    /// <param name="ct"> cancelation token</param>
    /// <returns></returns>
    Task BloquearVehiculoAsync(string vehiculoId, CancellationToken ct = default);
    
    /// <summary>
    /// cambia el estado del vehiculo al deseado
    /// </summary>
    /// <param name="id"></param>
    /// <param name="estadoEnum"></param>
    /// <returns></returns>
    public Task<Vehiculo?> UpdateEstado(string id, EstadoEnum estadoEnum ,CancellationToken ct = default);
}