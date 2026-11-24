using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Repositories.Taller;

public class ParteRepository(AppDbContext context) : IParteRepository
{
    private static readonly Estado[] EstadosReservaViva =
    [
        Estado.Pendiente,
        Estado.Confirmada,
        Estado.EnCurso
    ];

    public async Task<List<Parte>> GetAllAsync(CancellationToken ct = default) =>
        await context.Partes
            .AsNoTracking()
            .Include(p => p.Vehiculo)
            .OrderByDescending(p => p.FechaApertura)
            .ToListAsync(cancellationToken: ct);

    public async Task<Parte?> GetByIdAsync(long id, CancellationToken ct = default) =>
        await context.Partes
            .Include(p => p.Vehiculo)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken: ct);

    public async Task<Parte> CreateAsync(Parte obj, CancellationToken ct = default)
    {
        await context.Partes.AddAsync(obj, ct);
        await context.SaveChangesAsync(ct);
        return obj;
    }

    public async Task<Parte?> UpdateAsync(Parte obj, CancellationToken ct = default)
    {
        var existe = await context.Partes.AnyAsync(p => p.Id == obj.Id, cancellationToken: ct);
        if (!existe) return null;

        context.Partes.Update(obj);
        await context.SaveChangesAsync(ct);
        return obj;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var parte = await context.Partes.FindAsync(id, ct);
        if (parte is null) return false;

        context.Partes.Remove(parte);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> TieneParteAbiertoAsync(string vehiculoId, CancellationToken ct = default) =>
        await context.Partes
            .AnyAsync(p => p.VehiculoId == vehiculoId && p.Estado == EstadoParte.Abierto, cancellationToken: ct);

    public async Task<Trabajador?> GetTrabajadorByIdAsync(long trabajadorId, CancellationToken ct = default) =>
        await context.Trabajadores
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == trabajadorId, cancellationToken: ct);

    public async Task<Vehiculo?> GetVehiculoByIdAsync(string vehiculoId, CancellationToken ct = default) =>
        await context.Vehiculos
            .FirstOrDefaultAsync(v => v.Id == vehiculoId, cancellationToken: ct);

    public async Task<List<Reserva>> GetReservasVivasByVehiculoIdAsync(string vehiculoId, CancellationToken ct = default) =>
        await context.Reservas
            .AsNoTracking()
            .Where(r => r.VehiculoId == vehiculoId && EstadosReservaViva.Contains(r.Estado))
            .ToListAsync(cancellationToken: ct);

    public async Task UpdateVehiculoAsync(Vehiculo vehiculo, CancellationToken ct = default)
    {
        context.Vehiculos.Update(vehiculo);
        await context.SaveChangesAsync(ct);
    }
}