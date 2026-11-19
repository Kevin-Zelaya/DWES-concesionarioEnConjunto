using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

public interface ILineaReservaService
{
    public Task<Result<List<LineaReserva>, DomainError>> CreateLineasReservaAsync(List<LineaReserva> lineas, int dias, CancellationToken ct = default);

    Task<Result<LineaReserva, DomainError>> GetLineaReservaByIdAsync(long id, CancellationToken ct = default);

    Task<Result<List<LineaReserva>, DomainError>> GetLineasByReservaIdAsync(long reservaId, CancellationToken ct = default);

    Task<Result<LineaReserva, DomainError>> UpdateLineaReservaAsync(long id, LineaReserva linea, CancellationToken ct = default);

    Task<Result<bool, DomainError>> DeleteLineaReservaAsync(long id, CancellationToken ct = default);
    public decimal CalculateTotalByLineas(List<LineaReserva> lineas);
}