public interface ILineaReservaRepository
{
    Task<LineaReserva> CreateAsync(LineaReserva linea, CancellationToken ct = default);
    public Task<List<LineaReserva>> CreateRangeAsync(List<LineaReserva> lineas, CancellationToken ct = default);
    Task<LineaReserva?> GetByIdAsync(long id, CancellationToken ct = default);

    Task<List<LineaReserva>> GetByReservaIdAsync(long reservaId, CancellationToken ct = default);

    Task<LineaReserva?> UpdateAsync(long id, LineaReserva linea, CancellationToken ct = default);

    Task<bool> DeleteAsync(long id, CancellationToken ct = default);
}