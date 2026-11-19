public interface IExtraRepository
{
    // CREATE
    Task<Extra> CreateAsync(Extra extra, CancellationToken ct = default);

    // READ
    Task<Extra?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<Extra>> GetAllAsync(CancellationToken ct = default);

    // UPDATE
    Task<Extra?> UpdateAsync(long id, Extra extra, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(long id, CancellationToken ct = default);
}