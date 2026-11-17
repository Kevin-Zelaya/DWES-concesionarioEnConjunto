using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models.Reservas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

public class ReservaRepository(
    AppDbContext _context
) : IReservaRepository
{

   
    /// ======================
    /// CREATE
    /// ======================
    public async Task<Reserva> CreateAsync(
        Reserva reserva,
        CancellationToken ct = default)
    {
        await _context.Reservas.AddAsync(reserva, ct);
        await _context.SaveChangesAsync(ct);

        return reserva;
    }
    /// ======================
    /// READ
    /// ======================
    public async Task<Reserva?> GetReservaByCodigoReferenciaAsync(string codigo, CancellationToken ct = default)
    {
        return await _context.Reservas
            .Include(r => r.Lineas)
            .FirstOrDefaultAsync(r => r.CodigoRefencia == codigo);
    }
    /// <inheritdoc cref="IReservaRepository.GetByIdAsync(long, CancellationToken)"/>
    public async Task<Reserva?> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        return await _context.Reservas
            .Include(r => r.Lineas)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }
    /// <inheritdoc cref="IReservaRepository.GetPaginatedVehiculosAsync(int, int, CancellationToken)"/>
    public async Task<(List<Reserva> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)> GetPaginatedVehiculosAsync
    (int pageNumber, int pageSize, CancellationToken ct = default)
    {
        return await GetPaginatedVehiculosAsync(pageNumber, pageSize, 0, false, ct); 
    }
    /// <inheritdoc cref="IReservaRepository.GetPaginatedVehiculosByCustomerIdAsync(long, int, int, CancellationToken)"/>
    public async Task<(List<Reserva> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)> GetPaginatedVehiculosByCustomerIdAsync
    (long id, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        return await GetPaginatedVehiculosAsync(pageNumber, pageSize, id, false, ct); 
    }
    /// <inheritdoc cref="IReservaRepository.GetPaginatedVehiculosByEmployeeIdAsync(long, int, int, CancellationToken)"/>
    public async Task<(List<Reserva> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)> GetPaginatedVehiculosByEmployeeIdAsync
    (long id, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        return await GetPaginatedVehiculosAsync(pageNumber, pageSize, id, true, ct);
    }
    // Reutilización para no repetir copia
    private async Task<(List<Reserva> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)> GetPaginatedVehiculosAsync
    (int pageNumber, int pageSize, long id = 0, bool isEmployee = true, CancellationToken ct = default)
    {
        // Obtener todos los registros disponibles por el id del trabajador
        var query = _context.Reservas
            .Include(r => r.Lineas)
            .AsNoTracking();
            
        if(id > 0) // si se le asigna un id se aplica un filtrado por id
            if(isEmployee) // true por defecto lo que asigna al filtrado el id del empleado
                query.Where(r => r.TrabajadorId == id);
            else
                query.Where(r => r.ClienteId == id);

        // Obtener el total de registros antes de aplicar paginación
        int count = await query.CountAsync(ct);

        // Calcular el número total de páginas       
        int totalPages = (int)Math.Ceiling((double)count / pageSize);

        List<Reserva> reservas = await query
            .OrderByDescending(r => r.FechaCreacion)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);


        // Comprobar páginas anterior y siguiente
        bool hasPreviousPage = pageNumber > 1;
        bool hasNextPage = pageNumber < totalPages;

        return (reservas, count, totalPages, hasPreviousPage, hasNextPage);
    }
    /// <inheritdoc cref="IReservaRepository.GetAllAsync(CancellationToken)"/>
    public async Task<IEnumerable<Reserva>> GetAllAsync(
        CancellationToken ct = default)
    {
        return await _context.Reservas
            .AsNoTracking()
            .ToListAsync(ct);
    }
    /// <inheritdoc cref="IReservaRepository.UpdateAsync(Reserva, CancellationToken)"/>
    public async Task<Reserva?> UpdateAsync(
        Reserva reserva,
        CancellationToken ct = default)
    {
        var existing = await _context.Reservas
            .FirstOrDefaultAsync(r => r.Id == reserva.Id, ct);

        if (existing is null)
            return null;

        existing.VehiculoId = reserva.VehiculoId;
        existing.FechaRecogida = reserva.FechaRecogida;
        existing.FechaDevolucion = reserva.FechaDevolucion;
        existing.Estado = reserva.Estado;
        existing.PrecioTotal = reserva.PrecioTotal;
        existing.PagoId = reserva.PagoId;
        existing.Notas = reserva.Notas;

        await _context.SaveChangesAsync(ct);

        return existing;
    }
    public async Task<Reserva?> UpdateStatusAsync(
        Reserva reserva,
        CancellationToken ct = default)
    {
        var existing = await _context.Reservas
            .FirstOrDefaultAsync(r => r.Id == reserva.Id, ct);

        if (existing is null)
            return null;

        existing.Estado = reserva.Estado;

        await _context.SaveChangesAsync(ct);

        return existing;
    }

    /// <inheritdoc cref="IReservaRepository.ExistsAsync(long, CancellationToken)"/>
    public async Task<bool> ExistsAsync(
        long id,
        CancellationToken ct = default)
    {
        return await _context.Reservas
            .AnyAsync(r => r.Id == id, ct);
    }
}
