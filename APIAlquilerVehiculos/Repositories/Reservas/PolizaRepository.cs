using APIAlquilerVehiculos.Entities;
using Microsoft.EntityFrameworkCore;

public class PolizaRepository(AppDbContext context) : IPolizaRepository
{
    // CREATE
    public async Task<Poliza> CreateAsync(
        Poliza poliza,
        CancellationToken ct = default)
    {
        await context.Polizas.AddAsync(poliza, ct);
        await context.SaveChangesAsync(ct);
        return poliza;
    }

    // READ
    public async Task<Poliza?> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        return await context.Polizas
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<List<Poliza>> GetAllAsync(
        CancellationToken ct = default)
    {
        return await context.Polizas
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // UPDATE
    public async Task<Poliza?> UpdateAsync(
        long id,
        Poliza poliza,
        CancellationToken ct = default)
    {
        var existente = await context.Polizas
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (existente is null)
            return null;

        existente.Nombre = poliza.Nombre;
        existente.Descripcion = poliza.Descripcion;
        existente.IncrementoDiario = poliza.IncrementoDiario;
        existente.Activa = poliza.Activa;

        await context.SaveChangesAsync(ct);

        return existente;
    }

    // DELETE
    public async Task<bool> DeleteAsync(
        long id,
        CancellationToken ct = default)
    {
        var poliza = await context.Polizas
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (poliza is null)
            return false;

        poliza.Activa = false;

        await context.SaveChangesAsync(ct);

        return true;
    }
}