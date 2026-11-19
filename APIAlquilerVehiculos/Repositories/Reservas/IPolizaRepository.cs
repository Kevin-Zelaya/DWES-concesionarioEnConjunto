public interface IPolizaRepository
{
    // CREATE
    Task<Poliza> CreateAsync(Poliza poliza, CancellationToken ct = default);

    // READ
    Task<Poliza?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<Poliza>> GetAllAsync(CancellationToken ct = default);

    // UPDATE
    Task<Poliza?> UpdateAsync(long id, Poliza poliza, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(long id, CancellationToken ct = default);
}