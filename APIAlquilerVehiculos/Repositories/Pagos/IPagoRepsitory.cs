
using APIAlquilerVehiculos.Models.Reservas;

namespace APIAlquilerVehiculos.Repositories.Pagos;

public interface IPagoRepository
{
    Task<Reserva?> GetReservaByIdAsync(
        long reservaId,
        CancellationToken ct = default);

    Task AddPagoAsync(
        Pago pago,
        CancellationToken ct = default);
}
