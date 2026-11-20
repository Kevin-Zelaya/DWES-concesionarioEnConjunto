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
    /// Obtiene una reservfa mediante su codigo de referencia
    /// </summary>
    /// <param name="codigo"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<Reserva?> GetReservaByCodigoReferenciaAsync(string codigo, CancellationToken ct = default);

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
    /// Actualiza las fechas de inicio y fin de una reserva.
    /// </summary>
    /// <param name="CodigoReferencia">Código de referencia de la reserva.</param>
    /// <param name="request">Datos con las nuevas fechas de la reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Reserva actualizada o null si no existe.</returns>
    Task<Reserva?> UpdateFechasAsync(string CodigoReferencia, ReservaUpdateFechaRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Actualiza la nota asociada a una reserva.
    /// </summary>
    /// <param name="CodigoReferencia">Código de referencia de la reserva.</param>
    /// <param name="request">Datos con la nueva nota de la reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Reserva actualizada o null si no existe.</returns>
    Task<Reserva?> UpdateNotaAsync(string CodigoReferencia, ReservaUpdateNotaRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Actualiza la información del pago de una reserva.
    /// </summary>
    /// <param name="CodigoReferencia">Código de referencia de la reserva.</param>
    /// <param name="request">Datos con la nueva información del pago.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Reserva actualizada o null si no existe.</returns>
    Task<Reserva?> UpdatePagoAsync(string CodigoReferencia, ReservaUpdatePagoRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Actualiza el estado de una reserva.
    /// </summary>
    /// <param name="CodigoReferencia">Código de referencia de la reserva.</param>
    /// <param name="request">Datos con el nuevo estado de la reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Reserva actualizada o null si no existe.</returns>
    Task<Reserva?> UpdateStatusAsync(string CodigoReferencia, ReservaUpdateStatusRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Actualiza el vehículo asociado a una reserva.
    /// </summary>
    /// <param name="CodigoReferencia">Código de referencia de la reserva.</param>
    /// <param name="request">Datos con el nuevo vehículo asignado.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Reserva actualizada o null si no existe.</returns>
    Task<Reserva?> UpdateVehicleAsync(string CodigoReferencia, ReservaUpdateVehicleRequestDto request, CancellationToken ct = default);

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
    Task<(List<Reserva> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)> GetPaginatedReservasAsync(int pageNumber, int pageSize, CancellationToken ct = default);

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
    Task<(List<Reserva> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)> GetPaginatedReservasByCustomerIdAsync(long id, int pageNumber, int pageSize, CancellationToken ct = default);

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
    Task<(List<Reserva> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)> GetPaginatedReservasByEmployeeIdAsync(long id, int pageNumber, int pageSize, CancellationToken ct = default);

   Task<bool> ExistsReservaSolapadaAsync(
        string vehiculoId,
        DateTime fechaRecogida,
        DateTime fechaDevolucion,
        long? reservaIdExcluir = null,
        CancellationToken ct = default);
    }
