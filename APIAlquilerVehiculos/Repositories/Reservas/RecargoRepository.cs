using APIAlquilerVehiculos.Entities;
using Microsoft.EntityFrameworkCore;

public class RecargoRepository(AppDbContext context) : IRecargoRepository
{
    // CREATE
    public async Task<Recargo> CreateAsync(
        Recargo recargo,
        CancellationToken ct = default)
    {
        await context.Recargos.AddAsync(recargo, ct);
        await context.SaveChangesAsync(ct);

        return recargo;
    }

    // READ
    public async Task<Recargo?> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        return await context.Recargos
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<List<Recargo>> GetAllAsync(
        CancellationToken ct = default)
    {
        return await context.Recargos
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // UPDATE
    public async Task<Recargo?> UpdateAsync(
        long id,
        Recargo recargo,
        CancellationToken ct = default)
    {
        var existente = await context.Recargos
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (existente is null)
            return null;

        existente.Nombre = recargo.Nombre;
        existente.Descripcion = recargo.Descripcion;
        existente.TipoPrecio = recargo.TipoPrecio;
        existente.Importe = recargo.Importe;
        existente.Activa = recargo.Activa;

        await context.SaveChangesAsync(ct);

        return existente;
    }

    // DELETE
    public async Task<bool> DeleteAsync(
        long id,
        CancellationToken ct = default)
    {
        var recargo = await context.Recargos
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (recargo is null)
            return false;

        recargo.Activa = false;

        await context.SaveChangesAsync(ct);

        return true;
    }
}