using APIAlquilerVehiculos.Entities;
using Microsoft.EntityFrameworkCore;

public class ExtraRepository(AppDbContext context) : IExtraRepository
{
    // CREATE
    public async Task<Extra> CreateAsync(
        Extra extra,
        CancellationToken ct = default)
    {
        await context.Extras.AddAsync(extra, ct);
        await context.SaveChangesAsync(ct);
        return extra;
    }

    // READ
    public async Task<Extra?> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        return await context.Extras
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<List<Extra>> GetAllAsync(
        CancellationToken ct = default)
    {
        return await context.Extras
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // UPDATE
    public async Task<Extra?> UpdateAsync(
        long id,
        Extra extra,
        CancellationToken ct = default)
    {
        var existente = await context.Extras
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (existente is null)
            return null;

        existente.Nombre = extra.Nombre;
        existente.Descripcion = extra.Descripcion;
        existente.Precio = extra.Precio;
        existente.TipoPrecio = extra.TipoPrecio;
        existente.Activa = extra.Activa;

        await context.SaveChangesAsync(ct);

        return existente;
    }

    // DELETE
    public async Task<bool> DeleteAsync(
        long id,
        CancellationToken ct = default)
    {
        var extra = await context.Extras
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (extra is null)
            return false;

        context.Extras.Remove(extra);
        await context.SaveChangesAsync(ct);

        return true;
    }
}