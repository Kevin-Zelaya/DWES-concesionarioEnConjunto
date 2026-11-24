using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

/// <summary>
/// Define las operaciones de lógica de negocio para la gestión de líneas de reserva.
/// </summary>
public interface ILineaReservaService
{
    /// <summary>
    /// Crea varias líneas de reserva, agrupando las que tengan características equivalentes y calculando sus subtotales.
    /// </summary>
    /// <param name="lineas">Lista de líneas de reserva que se desean crear.</param>
    /// <param name="dias">Número de días de alquiler.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Resultado con la lista de líneas creadas o un error de dominio.</returns>
    Task<Result<List<LineaReserva>, DomainError>> CreateLineasReservaAsync(List<LineaReserva> lineas, int dias, CancellationToken ct = default);

    /// <summary>
    /// Obtiene una línea de reserva por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la línea de reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Resultado con la línea encontrada o un error de dominio.</returns>
    Task<Result<LineaReserva, DomainError>> GetLineaReservaByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las líneas asociadas a una reserva.
    /// </summary>
    /// <param name="reservaId">Identificador de la reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Resultado con la lista de líneas o un error de dominio.</returns>
    Task<Result<List<LineaReserva>, DomainError>> GetLineasByReservaIdAsync(long reservaId, CancellationToken ct = default);

    /// <summary>
    /// Actualiza una línea de reserva existente.
    /// </summary>
    /// <param name="id">Identificador de la línea que se desea actualizar.</param>
    /// <param name="linea">Nuevos datos de la línea de reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Resultado con la línea actualizada o un error de dominio.</returns>
    Task<Result<LineaReserva, DomainError>> UpdateLineaReservaAsync(long id, LineaReserva linea, CancellationToken ct = default);

    /// <summary>
    /// Elimina una línea de reserva por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la línea que se desea eliminar.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Resultado indicando si se ha eliminado correctamente o un error de dominio.</returns>
    Task<Result<bool, DomainError>> DeleteLineaReservaAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Calcula el importe total sumando los subtotales de las líneas de reserva.
    /// </summary>
    /// <param name="lineas">Lista de líneas cuyos subtotales se desean sumar.</param>
    /// <returns>Importe total de las líneas de reserva.</returns>
    decimal CalculateTotalByLineas(List<LineaReserva> lineas);
}