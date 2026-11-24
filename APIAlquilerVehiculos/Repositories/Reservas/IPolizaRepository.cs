/// <summary>
/// Define las operaciones de acceso a datos para la gestión de pólizas.
/// </summary>
public interface IPolizaRepository
{
    // CREATE

    /// <summary>
    /// Crea una nueva póliza.
    /// </summary>
    /// <param name="poliza">Póliza que se desea crear.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>La póliza creada.</returns>
    Task<Poliza> CreateAsync(Poliza poliza, CancellationToken ct = default);

    // READ

    /// <summary>
    /// Obtiene una póliza por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la póliza.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>La póliza encontrada o null si no existe.</returns>
    Task<Poliza?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todas las pólizas registradas.
    /// </summary>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Lista de pólizas.</returns>
    Task<List<Poliza>> GetAllAsync(CancellationToken ct = default);

    // UPDATE

    /// <summary>
    /// Actualiza los datos de una póliza existente.
    /// </summary>
    /// <param name="id">Identificador de la póliza que se desea actualizar.</param>
    /// <param name="poliza">Nuevos datos de la póliza.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>La póliza actualizada o null si no existe.</returns>
    Task<Poliza?> UpdateAsync(long id, Poliza poliza, CancellationToken ct = default);

    // DELETE

    /// <summary>
    /// Elimina una póliza por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la póliza que se desea eliminar.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns> true si se ha eliminado correctamente; false si no existe.</returns>
    Task<bool> DeleteAsync(long id, CancellationToken ct = default);
}