using System.Data;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Reservas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Serilog;

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
    /// <inheritdoc cref="IReservaRepository.GetPaginatedReservasAsync(int, int, CancellationToken)"/>
    public async Task<(List<Reserva> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)> GetPaginatedReservasAsync
    (int pageNumber, int pageSize, CancellationToken ct = default)
    {
        return await GetPaginatedReservasAsync(pageNumber, pageSize, 0, false, ct);
    }
    /// <inheritdoc cref="IReservaRepository.GetPaginatedReservasByCustomerIdAsync(long, int, int, CancellationToken)"/>
    public async Task<(List<Reserva> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)> GetPaginatedReservasByCustomerIdAsync
    (long id, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        return await GetPaginatedReservasAsync(pageNumber, pageSize, id, false, ct);
    }
    /// <inheritdoc cref="IReservaRepository.GetPaginatedReservasByEmployeeIdAsync(long, int, int, CancellationToken)"/>
    public async Task<(List<Reserva> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)> GetPaginatedReservasByEmployeeIdAsync
    (long id, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        return await GetPaginatedReservasAsync(pageNumber, pageSize, id, true, ct);
    }
    /// <summary>
    /// Privado, pertmite reutilzar la lógica del paginado
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="id"></param>
    /// <param name="isEmployee"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<(List<Reserva> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)> GetPaginatedReservasAsync
    (int pageNumber, int pageSize, long id = 0, bool isEmployee = true, CancellationToken ct = default)
    {
        // Obtener todos los registros disponibles por el id del trabajador
        var query = _context.Reservas
            .Include(r => r.Lineas)
            .AsNoTracking();

        if (id > 0) // si se le asigna un id se aplica un filtrado por id
            if (isEmployee) // true por defecto lo que asigna al filtrado el id del empleado
                query = query.Where(r => r.TrabajadorId == id);
            else
                query = query.Where(r => r.ClienteId == id);

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
    /// ======================
    /// UPDATE
    /// ======================
    public async Task<Reserva?> UpdateNotaAsync(
        string CodigoReferencia,
        ReservaUpdateNotaRequestDto request,
        CancellationToken ct = default)
    {
        var response = await _context.Reservas.FirstOrDefaultAsync(r => r.CodigoRefencia == CodigoReferencia);
        if (response is null) return null;
        response.Notas = request.Nota;
        var toReturn = await UpdateAsync(response, request.RowVersion, ct);
        return toReturn;
    }
    /// <inheritdoc cref="IReservaRepository.UpdatePagoAsync(string, ReservaUpdatePagoRequestDto, CancellationToken)"/>
    public async Task<Reserva?> UpdatePagoAsync(
        string CodigoReferencia,
        ReservaUpdatePagoRequestDto request,
        CancellationToken ct = default)
    {
        var response = await _context.Reservas.FirstOrDefaultAsync(r => r.CodigoRefencia == CodigoReferencia);
        if (response is null) return null;
        response.PagoId = request.PagoId;
        var toReturn = await UpdateAsync(response, request.RowVersion, ct);
        await _context.Entry(toReturn)
                .Reference(r => r.Pago)
            .LoadAsync(ct);
        return toReturn;
    }
    /// <inheritdoc cref="IReservaRepository.UpdateFechasAsync(string, ReservaUpdateFechaRequestDto, CancellationToken)"/>
    public async Task<Reserva?> UpdateFechasAsync(
        string CodigoReferencia,
        ReservaUpdateFechaRequestDto request,
        CancellationToken ct = default)
    {
        var response = await _context.Reservas.FirstOrDefaultAsync(r => r.CodigoRefencia == CodigoReferencia);
        if (response is null) return null;
        response.FechaRecogida = request.FechaRecogida;
        response.FechaDevolucion = request.FechaDevolucion;
        return await UpdateAsync(response, request.RowVersion, ct);
    }
    /// <inheritdoc cref="IReservaRepository.UpdateStatusAsync(string, ReservaUpdateStatusRequestDto, CancellationToken)"/>
    public async Task<Reserva?> UpdateStatusAsync(
        string CodigoReferencia,
        ReservaUpdateStatusRequestDto request,
        CancellationToken ct = default)
    {
        var response = await _context.Reservas.FirstOrDefaultAsync(r => r.CodigoRefencia == CodigoReferencia);
        if (response is null) return null;
        response.Estado = request.Estado;
        return await UpdateAsync(response, request.RowVersion, ct);
    }
    /// <inheritdoc cref="IReservaRepository.UpdateVehicleAsync(string, ReservaUpdateVehicleRequestDto, CancellationToken)"/>
    public async Task<Reserva?> UpdateVehicleAsync(
        string CodigoReferencia,
        ReservaUpdateVehicleRequestDto request,
        CancellationToken ct = default)
    {
        var response = await _context.Reservas.FirstOrDefaultAsync(r => r.CodigoRefencia == CodigoReferencia);
        if (response is null) return null;
        response.VehiculoId = request.VehiculoId;
        return await UpdateAsync(response, request.RowVersion, ct);
    }
    /// <summary>
    /// Metodo reutilizable que permite definir aqui el control de concurrencia
    /// para reutilizarlo en todos lus update
    /// </summary>
    /// <param name="Reserva"></param>
    /// <param name="RowVersion"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<Reserva> UpdateAsync(
        Reserva Reserva,
        uint RowVersion,
        CancellationToken ct = default)
    {
        try
        {
            _context.Entry(Reserva)
                .Property(f => f.RowVersion)
                .OriginalValue = RowVersion;
            await _context.SaveChangesAsync(ct);
            await _context.Entry(Reserva)
                .Reference(r => r.Vehiculo)
                .LoadAsync();
        }
        catch (DBConcurrencyException)
        {
            Log.Error("[REPOSITORY-UPDATE] Error de concurrencia, al actualizar la reserva con codigó de referencia: {cr}", Reserva.CodigoRefencia);
            throw;
        }
        return Reserva;
    }

    /// <inheritdoc cref="IReservaRepository.ExistsAsync(long, CancellationToken)"/>
    public async Task<bool> ExistsAsync(
        long id,
        CancellationToken ct = default)
    {
        return await _context.Reservas
            .AnyAsync(r => r.Id == id, ct);
    }

    /// <summary>
    /// Comprueba si un vehículo tiene alguna reserva que se solape
    /// con el rango de fechas indicado.
    /// </summary>
    public async Task<bool> ExistsReservaSolapadaAsync(
        string vehiculoId,
        DateTime fechaRecogida,
        DateTime fechaDevolucion,
        long? reservaIdExcluir = null,
        CancellationToken ct = default)
    {
        return await _context.Reservas
            .AsNoTracking()
            .AnyAsync(r =>
                r.VehiculoId == vehiculoId &&
                (
                    r.Estado == Estado.Pendiente ||
                    r.Estado == Estado.Confirmada ||
                    r.Estado == Estado.EnCurso
                ) &&
                (reservaIdExcluir == null || r.Id != reservaIdExcluir) &&
                r.FechaRecogida < fechaDevolucion &&
                r.FechaDevolucion > fechaRecogida,
                ct);
    }

    /// =======================
    /// AUXILIAR
    /// =======================
    public async Task<Oficina?> GetOficinaById(
        long id,
        CancellationToken ct = default)
    {
        return await _context.Oficinas
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<bool> ExistsReservasActivasAsync(
        string vehiculoId,
        long reservaIdExcluir,
        CancellationToken ct = default)
    {
        return await _context.Reservas
            .AsNoTracking()
            .AnyAsync(r =>
                r.VehiculoId == vehiculoId &&
                r.Id != reservaIdExcluir &&
                (
                    r.Estado == Estado.Pendiente ||
                    r.Estado == Estado.Confirmada ||
                    r.Estado == Estado.EnCurso
                ),
                ct);
    }
}
