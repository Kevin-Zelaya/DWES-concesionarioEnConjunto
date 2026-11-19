public interface IRecargoRepository
{
    // CREATE
    Task<Recargo> CreateAsync(Recargo recargo, CancellationToken ct = default);

    // READ
    Task<Recargo?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<Recargo>> GetAllAsync(CancellationToken ct = default);

    // UPDATE
    Task<Recargo?> UpdateAsync(long id, Recargo recargo, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(long id, CancellationToken ct = default);
}