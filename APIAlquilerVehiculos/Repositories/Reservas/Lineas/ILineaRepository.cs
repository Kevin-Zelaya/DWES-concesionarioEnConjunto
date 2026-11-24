/// <summary>
/// Define las operaciones de acceso a datos para la gestión de las líneas de reserva.
/// </summary>
public interface ILineaReservaRepository
{
    /// <summary>
    /// Crea una nueva línea de reserva.
    /// </summary>
    /// <param name="linea">Línea de reserva que se desea crear.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>La línea de reserva creada.</returns>
    Task<LineaReserva> CreateAsync(LineaReserva linea, CancellationToken ct = default);

    /// <summary>
    /// Crea varias líneas de reserva en una misma operación.
    /// </summary>
    /// <param name="lineas">Lista de líneas de reserva que se desean crear.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Lista de líneas de reserva creadas.</returns>
    Task<List<LineaReserva>> CreateRangeAsync(List<LineaReserva> lineas, CancellationToken ct = default);

    /// <summary>
    /// Obtiene una línea de reserva por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la línea de reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>La línea encontrada o null si no existe.</returns>
    Task<LineaReserva?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las líneas asociadas a una reserva.
    /// </summary>
    /// <param name="reservaId">Identificador de la reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Lista de líneas pertenecientes a la reserva.</returns>
    Task<List<LineaReserva>> GetByReservaIdAsync(long reservaId, CancellationToken ct = default);

    /// <summary>
    /// Actualiza una línea de reserva existente.
    /// </summary>
    /// <param name="id">Identificador de la línea que se desea actualizar.</param>
    /// <param name="linea">Nuevos datos de la línea de reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>La línea actualizada o null si no existe.</returns>
    Task<LineaReserva?> UpdateAsync(long id, LineaReserva linea, CancellationToken ct = default);

    /// <summary>
    /// Elimina una línea de reserva por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la línea que se desea eliminar.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>true si se ha eliminado correctamente; false si no existe.</returns>
    Task<bool> DeleteAsync(long id, CancellationToken ct = default);
}