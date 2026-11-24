using APIAlquilerVehiculos.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Implementa las operaciones de acceso a datos para la gestión de líneas de reserva.
/// </summary>
/// <param name="_context">Contexto de base de datos de la aplicación.</param>
public class LineaReservaRepository(AppDbContext _context) : ILineaReservaRepository
{
    /// <inheritdoc cref="ILineaReservaRepository.CreateAsync(LineaReserva, CancellationToken)"/>
    public async Task<LineaReserva> CreateAsync(LineaReserva linea, CancellationToken ct = default)
    {
        await _context.Lineas.AddAsync(linea, ct);
        await _context.SaveChangesAsync(ct);
        return linea;
    }

    /// <inheritdoc cref="ILineaReservaRepository.CreateRangeAsync(List{LineaReserva}, CancellationToken)"/>
    public async Task<List<LineaReserva>> CreateRangeAsync(List<LineaReserva> lineas, CancellationToken ct = default)
    {
        await _context.Lineas.AddRangeAsync(lineas, ct);
        // Los cambios se guardan al actualizar la reserva.
        // await _context.SaveChangesAsync(ct);

        return lineas;
    }

    /// <inheritdoc cref="ILineaReservaRepository.GetByIdAsync(long, CancellationToken)"/>
    public async Task<LineaReserva?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _context.Lineas
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    /// <inheritdoc cref="ILineaReservaRepository.GetByReservaIdAsync(long, CancellationToken)"/>
    public async Task<List<LineaReserva>> GetByReservaIdAsync(long reservaId, CancellationToken ct = default)
    {
        return await _context.Lineas
            .Where(l => l.ReservaId == reservaId)
            .ToListAsync(ct);
    }

    /// <inheritdoc cref="ILineaReservaRepository.UpdateAsync(long, LineaReserva, CancellationToken)"/>
    public async Task<LineaReserva?> UpdateAsync(long id, LineaReserva linea, CancellationToken ct = default)
    {
        var response = await _context.Lineas
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (response is null)
            return null;

        response.Cantidad = linea.Cantidad;

        return response;
    }

    /// <inheritdoc cref="ILineaReservaRepository.DeleteAsync(long, CancellationToken)"/>
    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var response = await _context.Lineas
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (response is null)
            return false;

        _context.Lineas.Remove(response);

        return true;
    }
}