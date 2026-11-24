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

    public async Task<List<Parte>> GetAllAsync() =>
        await context.Partes
            .AsNoTracking()
            .Include(p => p.Vehiculo)
            .OrderByDescending(p => p.FechaApertura)
            .ToListAsync();

    public async Task<Parte?> GetByIdAsync(long id) =>
        await context.Partes
            .Include(p => p.Vehiculo)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Parte> CreateAsync(Parte obj)
    {
        await context.Partes.AddAsync(obj);
        await context.SaveChangesAsync();
        return obj;
    }

    public async Task<Parte?> UpdateAsync(Parte obj)
    {
        var existe = await context.Partes.AnyAsync(p => p.Id == obj.Id);
        if (!existe) return null;

        context.Partes.Update(obj);
        await context.SaveChangesAsync();
        return obj;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var parte = await context.Partes.FindAsync(id);
        if (parte is null) return false;

        context.Partes.Remove(parte);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TieneParteAbiertoAsync(string vehiculoId) =>
        await context.Partes
            .AnyAsync(p => p.VehiculoId == vehiculoId && p.Estado == EstadoParte.Abierto);

    public async Task<Trabajador?> GetTrabajadorByIdAsync(long trabajadorId) =>
        await context.Trabajadores
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == trabajadorId);

    public async Task<Vehiculo?> GetVehiculoByIdAsync(string vehiculoId) =>
        await context.Vehiculos
            .FirstOrDefaultAsync(v => v.Id == vehiculoId);

    public async Task<List<Reserva>> GetReservasVivasByVehiculoIdAsync(string vehiculoId) =>
        await context.Reservas
            .AsNoTracking()
            .Where(r => r.VehiculoId == vehiculoId && EstadosReservaViva.Contains(r.Estado))
            .ToListAsync();

    public async Task UpdateVehiculoAsync(Vehiculo vehiculo)
    {
        context.Vehiculos.Update(vehiculo);
        await context.SaveChangesAsync();
    }
}