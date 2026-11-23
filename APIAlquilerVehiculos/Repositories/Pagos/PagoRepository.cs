using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models.Reservas;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Repositories.Pagos;

public class PagoRepository(AppDbContext context) : IPagoRepository
{
    public async Task<Reserva?> GetReservaByIdAsync(
        long reservaId,
        CancellationToken ct = default)
    {
        return await context.Reservas
            .FirstOrDefaultAsync(r => r.Id == reservaId, ct);
    }

    public async Task AddPagoAsync(
        Pago pago,
        CancellationToken ct = default)
    {
        context.Pagos.Add(pago);

        await context.SaveChangesAsync(ct);
    }
}
