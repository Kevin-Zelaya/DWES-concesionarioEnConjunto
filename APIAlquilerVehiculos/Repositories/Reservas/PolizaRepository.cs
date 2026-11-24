using APIAlquilerVehiculos.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Implementa las operaciones de acceso a datos para la gestión de pólizas.
/// </summary>
/// <param name="context">Contexto de base de datos de la aplicación.</param>
public class PolizaRepository(AppDbContext context) : IPolizaRepository
{
    // CREATE

    /// <inheritdoc cref="IPolizaRepository.CreateAsync(Poliza, CancellationToken)"/>
    public async Task<Poliza> CreateAsync(Poliza poliza, CancellationToken ct = default)
    {
        await context.Polizas.AddAsync(poliza, ct);
        await context.SaveChangesAsync(ct);
        return poliza;
    }

    // READ

    /// <inheritdoc cref="IPolizaRepository.GetByIdAsync(long, CancellationToken)"/>
    public async Task<Poliza?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await context.Polizas
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    /// <inheritdoc cref="IPolizaRepository.GetAllAsync(CancellationToken)"/>
    public async Task<List<Poliza>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Polizas
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // UPDATE

    /// <inheritdoc cref="IPolizaRepository.UpdateAsync(long, Poliza, CancellationToken)"/>
    public async Task<Poliza?> UpdateAsync(long id, Poliza poliza, CancellationToken ct = default)
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

    /// <inheritdoc cref="IPolizaRepository.DeleteAsync(long, CancellationToken)"/>
    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
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