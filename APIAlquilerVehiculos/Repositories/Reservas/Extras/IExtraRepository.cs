/// <summary>
/// Define las operaciones de acceso a datos para la gestión de extras.
/// </summary>
public interface IExtraRepository
{
    // CREATE

    /// <summary>
    /// Crea un nuevo extra.
    /// </summary>
    /// <param name="extra">Extra que se desea crear.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El extra creado.</returns>
    Task<Extra> CreateAsync(Extra extra, CancellationToken ct = default);

    // READ

    /// <summary>
    /// Obtiene un extra por su identificador.
    /// </summary>
    /// <param name="id">Identificador del extra.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El extra encontrado o null si no existe.</returns>
    Task<Extra?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los extras registrados.
    /// </summary>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Lista de extras.</returns>
    Task<List<Extra>> GetAllAsync(CancellationToken ct = default);

    // UPDATE

    /// <summary>
    /// Actualiza los datos de un extra existente.
    /// </summary>
    /// <param name="id">Identificador del extra que se desea actualizar.</param>
    /// <param name="extra">Nuevos datos del extra.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El extra actualizado o null si no existe.</returns>
    Task<Extra?> UpdateAsync(long id, Extra extra, CancellationToken ct = default);

    // DELETE

    /// <summary>
    /// Elimina un extra mediante su identificador.
    /// </summary>
    /// <param name="id">Identificador del extra que se desea eliminar.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>true si se ha eliminado correctamente; false si no existe.</returns>
    Task<bool> DeleteAsync(long id, CancellationToken ct = default);
}