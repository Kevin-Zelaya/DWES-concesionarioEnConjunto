using APIAlquilerVehiculos.Entities;
using Microsoft.EntityFrameworkCore;

public class LineaReservaRepository(AppDbContext _context) : ILineaReservaRepository
{
    public async Task<LineaReserva> CreateAsync(LineaReserva linea, CancellationToken ct = default)
    {
        await _context.Lineas.AddAsync(linea, ct);
        await _context.SaveChangesAsync(ct);
        return linea;
    }

    public async Task<List<LineaReserva>> CreateRangeAsync(
        List<LineaReserva> lineas,
        CancellationToken ct = default)
    {
        await _context.Lineas.AddRangeAsync(lineas, ct);
        // los cambios se guardan al actulaizar la reserva
        // await _context.SaveChangesAsync(ct);

        return lineas;
    }

    public async Task<LineaReserva?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _context.Lineas
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public async Task<List<LineaReserva>> GetByReservaIdAsync(long reservaId, CancellationToken ct = default)
    {
        return await _context.Lineas
            .Where(l => l.ReservaId == reservaId)
            .ToListAsync(ct);
    }

    public async Task<LineaReserva?> UpdateAsync(long id, LineaReserva linea, CancellationToken ct = default)
    {
        var response = await _context.Lineas
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (response is null)
            return null;

        response.Cantidad = linea.Cantidad;

        return response;
    }

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