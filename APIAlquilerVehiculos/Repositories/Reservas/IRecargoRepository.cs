/// <summary>
/// Define las operaciones de acceso a datos para la gestión de recargos.
/// </summary>
public interface IRecargoRepository
{
    // CREATE

    /// <summary>
    /// Crea un nuevo recargo.
    /// </summary>
    /// <param name="recargo">Recargo que se desea crear.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El recargo creado.</returns>
    Task<Recargo> CreateAsync(Recargo recargo, CancellationToken ct = default);

    // READ

    /// <summary>
    /// Obtiene un recargo por su identificador.
    /// </summary>
    /// <param name="id">Identificador del recargo.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El recargo encontrado o null si no existe.</returns>
    Task<Recargo?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los recargos registrados.
    /// </summary>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Lista de recargos.</returns>
    Task<List<Recargo>> GetAllAsync(CancellationToken ct = default);

    // UPDATE

    /// <summary>
    /// Actualiza los datos de un recargo existente.
    /// </summary>
    /// <param name="id">Identificador del recargo que se desea actualizar.</param>
    /// <param name="recargo">Nuevos datos del recargo.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El recargo actualizado o null si no existe.</returns>
    Task<Recargo?> UpdateAsync(long id, Recargo recargo, CancellationToken ct = default);

    // DELETE

    /// <summary>
    /// Desactiva un recargo mediante su identificador.
    /// </summary>
    /// <param name="id">Identificador del recargo que se desea desactivar.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>true si se ha desactivado correctamente; false si no existe.</returns>
    Task<bool> DeleteAsync(long id, CancellationToken ct = default);
}