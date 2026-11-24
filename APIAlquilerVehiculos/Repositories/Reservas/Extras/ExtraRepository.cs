using APIAlquilerVehiculos.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Implementa las operaciones de acceso a datos para la gestión de extras.
/// </summary>
/// <param name="context">Contexto de base de datos de la aplicación.</param>
public class ExtraRepository(AppDbContext context) : IExtraRepository
{
    // CREATE

    /// <inheritdoc cref="IExtraRepository.CreateAsync(Extra, CancellationToken)"/>
    public async Task<Extra> CreateAsync(Extra extra, CancellationToken ct = default)
    {
        await context.Extras.AddAsync(extra, ct);
        await context.SaveChangesAsync(ct);
        return extra;
    }

    // READ

    /// <inheritdoc cref="IExtraRepository.GetByIdAsync(long, CancellationToken)"/>
    public async Task<Extra?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await context.Extras
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    /// <inheritdoc cref="IExtraRepository.GetAllAsync(CancellationToken)"/>
    public async Task<List<Extra>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Extras
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // UPDATE

    /// <inheritdoc cref="IExtraRepository.UpdateAsync(long, Extra, CancellationToken)"/>
    public async Task<Extra?> UpdateAsync(long id, Extra extra, CancellationToken ct = default)
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

    /// <inheritdoc cref="IExtraRepository.DeleteAsync(long, CancellationToken)"/>
    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        // Buscamos el extra
        var extra = await context.Extras
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (extra is null)
            return false;

        // Desactivamos el extra
        extra.Activa = false;

        await context.SaveChangesAsync(ct);

        return true;
    }
}