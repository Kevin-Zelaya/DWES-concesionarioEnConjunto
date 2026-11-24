using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Models.Reservas;
using CSharpFunctionalExtensions;

/// <summary>
/// Define las operaciones disponibles para la gestión de reservas.
/// </summary>
public interface IReservaService
{
    /// <summary>
    /// Crea una nueva reserva.
    /// </summary>
    /// <param name="reserva">Datos de la reserva que se desea crear.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El resultado de la creación de la reserva o un error de dominio.</returns>
    Task<Result<ReservaDto, DomainError>> CreateReservaAsync(ReservaCreateRequestDto reserva, long TrabajadorId, CancellationToken ct = default);

    /// <summary>
    /// Obtiene una reserva mediante su identificador.
    /// </summary>
    /// <param name="id">Identificador de la reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El resultado de la búsqueda de la reserva o un error de dominio.</returns>
    Task<Result<Reserva, DomainError>> GetReservaByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene una reserva mediante su código de referencia.
    /// </summary>
    /// <param name="codigo">Código de referencia de la reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El resultado de la búsqueda de la reserva o un error de dominio.</returns>
    Task<Result<Reserva, DomainError>> GetReservaByCodigoReferenciaAsync(string codigo, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las reservas registradas.
    /// </summary>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El resultado de la consulta con la lista de reservas o un error de dominio.</returns>
    Task<Result<List<ReservaDto>, DomainError>> GetAllReservasAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene las reservas de forma paginada.
    /// </summary>
    /// <param name="pageNumber">Número de página que se desea consultar.</param>
    /// <param name="pageSize">Cantidad de reservas por página.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El resultado de la consulta paginada de reservas o un error de dominio.</returns>
    Task<Result<PaginatedResouce<Reserva>, DomainError>> GetPaginatedReservasAsync(int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Obtiene las reservas de un cliente de forma paginada.
    /// </summary>
    /// <param name="id">Identificador del cliente.</param>
    /// <param name="pageNumber">Número de página que se desea consultar.</param>
    /// <param name="pageSize">Cantidad de reservas por página.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El resultado de la consulta paginada de reservas del cliente o un error de dominio.</returns>
    Task<Result<PaginatedResouce<Reserva>, DomainError>> GetPaginatedReservasByCustomerIdAsync(long id, int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Obtiene las reservas de un trabajador de forma paginada.
    /// </summary>
    /// <param name="id">Identificador del trabajador.</param>
    /// <param name="pageNumber">Número de página que se desea consultar.</param>
    /// <param name="pageSize">Cantidad de reservas por página.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El resultado de la consulta paginada de reservas del trabajador o un error de dominio.</returns>
    Task<Result<PaginatedResouce<Reserva>, DomainError>> GetPaginatedReservasByEmployeeIdAsync(long id, int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Actualiza las notas de una reserva.
    /// </summary>
    /// <param name="codigoReferencia">Código de referencia de la reserva que se desea actualizar.</param>
    /// <param name="request">Datos de las notas que se desean actualizar.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El resultado de la actualización de la reserva o un error de dominio.</returns>
    Task<Result<Reserva, DomainError>> UpdateNotaAsync(string codigoReferencia, ReservaUpdateNotaRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Actualiza las fechas de recogida y devolución de una reserva.
    /// </summary>
    /// <param name="codigoReferencia">Código de referencia de la reserva que se desea actualizar.</param>
    /// <param name="request">Nuevas fechas de recogida y devolución.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El resultado de la actualización de la reserva o un error de dominio.</returns>
    Task<Result<Reserva, DomainError>> UpdateFechasAsync(string codigoReferencia, ReservaUpdateFechaRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Actualiza el estado de una reserva.
    /// </summary>
    /// <param name="codigoReferencia">Código de referencia de la reserva que se desea actualizar.</param>
    /// <param name="request">Nuevo estado de la reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El resultado de la actualización de la reserva o un error de dominio.</returns>
    Task<Result<Reserva, DomainError>> UpdateStatusAsync(string codigoReferencia, ReservaUpdateStatusRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Actualiza el vehículo asignado a una reserva.
    /// </summary>
    /// <param name="codigoReferencia">Código de referencia de la reserva que se desea actualizar.</param>
    /// <param name="request">Datos del vehículo que se desea asignar.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El resultado de la actualización de la reserva o un error de dominio.</returns>
    Task<Result<Reserva, DomainError>> UpdateVehicleAsync(string codigoReferencia, ReservaUpdateVehicleRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Comprueba si existe una reserva mediante su identificador.
    /// </summary>
    /// <param name="id">Identificador de la reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El resultado de la comprobación de existencia o un error de dominio.</returns>
    Task<Result<bool, DomainError>> ExistsAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Procesa las reservas confirmadas cuyo plazo de recogida ha finalizado
    /// y que deben marcarse como no presentadas.
    /// </summary>
    /// <param name="ct">Token para cancelar la operación.</param>
    Task ProcesarReservasPendientesAsync(CancellationToken ct = default);

    /// <summary>
    /// Detecta las reservas en curso cuya fecha de devolución ha vencido
    /// y registra las incidencias correspondientes.
    /// </summary>
    /// <param name="ct">Token para cancelar la operación.</param>
    Task ProcesarDevolucionesTardiasAsync(CancellationToken ct = default);

    /// <summary>
    /// Detecta las reservas próximas cuya asignación de vehículo no está disponible
    /// y registra las incidencias correspondientes.
    /// </summary>
    /// <param name="ct">Token para cancelar la operación.</param>
    Task ProcesarVehiculosNoDisponiblesAsync(CancellationToken ct = default);
}