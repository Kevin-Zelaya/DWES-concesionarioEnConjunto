using APIAlquilerVehiculos.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Implementa las operaciones de acceso a datos para la gestión de recargos.
/// </summary>
/// <param name="context">Contexto de base de datos de la aplicación.</param>
public class RecargoRepository(AppDbContext context) : IRecargoRepository
{
    // CREATE

    /// <inheritdoc cref="IRecargoRepository.CreateAsync(Recargo, CancellationToken)"/>
    public async Task<Recargo> CreateAsync(Recargo recargo, CancellationToken ct = default)
    {
        await context.Recargos.AddAsync(recargo, ct);
        await context.SaveChangesAsync(ct);

        return recargo;
    }

    // READ

    /// <inheritdoc cref="IRecargoRepository.GetByIdAsync(long, CancellationToken)"/>
    public async Task<Recargo?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await context.Recargos
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    /// <inheritdoc cref="IRecargoRepository.GetAllAsync(CancellationToken)"/>
    public async Task<List<Recargo>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Recargos
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // UPDATE

    /// <inheritdoc cref="IRecargoRepository.UpdateAsync(long, Recargo, CancellationToken)"/>
    public async Task<Recargo?> UpdateAsync(long id, Recargo recargo, CancellationToken ct = default)
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

    /// <inheritdoc cref="IRecargoRepository.DeleteAsync(long, CancellationToken)"/>
    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
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