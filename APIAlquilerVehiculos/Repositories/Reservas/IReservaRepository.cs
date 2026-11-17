using APIAlquilerVehiculos.Models.Reservas;


/// <summary>
/// Define las operaciones de acceso a datos para la entidad Reserva.
/// </summary>
public interface IReservaRepository
{
    /// <summary>
    /// Obtiene una reserva mediante su identificador.
    /// </summary>
    /// <param name="id">Identificador de la reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>La reserva encontrada o null si no existe.</returns>
    Task<Reserva?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las reservas registradas.
    /// </summary>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Listado de reservas.</returns>
    Task<IEnumerable<Reserva>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Registra una nueva reserva.
    /// </summary>
    /// <param name="reserva">Datos de la reserva que se desea crear.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>La reserva creada.</returns>
    Task<Reserva> CreateAsync(Reserva reserva, CancellationToken ct = default);

    /// <summary>
    /// Actualiza los datos de una reserva existente.
    /// </summary>
    /// <param name="reserva">Reserva con los datos actualizados.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>La reserva actualizada o null si no existe.</returns>
    Task<Reserva?> UpdateAsync(Reserva reserva, CancellationToken ct = default);

    /// <summary>
    /// Comprueba si existe una reserva con el identificador indicado.
    /// </summary>
    /// <param name="id">Identificador de la reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>True si existe; false en caso contrario.</returns>
    Task<bool> ExistsAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las reservas de forma paginada.
    /// </summary>
    /// <param name="pageNumber">Número de página que se desea consultar.</param>
    /// <param name="pageSize">Cantidad de reservas por página.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>
    /// Tupla con la lista de reservas, el total de registros,
    /// el número de páginas y los indicadores de navegación.
    /// </returns>
    Task<(List<Reserva> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)> GetPaginatedVehiculosAsync(int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Obtiene las reservas de un cliente de forma paginada.
    /// </summary>
    /// <param name="id">Identificador del cliente.</param>
    /// <param name="pageNumber">Número de página que se desea consultar.</param>
    /// <param name="pageSize">Cantidad de reservas por página.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>
    /// Tupla con las reservas del cliente, el total de registros,
    /// el número de páginas y los indicadores de navegación.
    /// </returns>
    Task<(List<Reserva> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)> GetPaginatedVehiculosByCustomerIdAsync(long id, int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Obtiene las reservas de un trabajador de forma paginada.
    /// </summary>
    /// <param name="id">Identificador del trabajador.</param>
    /// <param name="pageNumber">Número de página que se desea consultar.</param>
    /// <param name="pageSize">Cantidad de reservas por página.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>
    /// Tupla con las reservas del trabajador, el total de registros,
    /// el número de páginas y los indicadores de navegación.
    /// </returns>
    Task<(List<Reserva> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)> GetPaginatedVehiculosByEmployeeIdAsync(long id, int pageNumber, int pageSize, CancellationToken ct = default);

}
