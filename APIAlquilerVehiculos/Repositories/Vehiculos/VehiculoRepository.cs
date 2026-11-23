using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Repositories.Vehiculos;

/// <summary>
/// Repositorio de Vehículos repository, acede a la base de datos desde el _context y gestiona la persistencia de datos
/// </summary>
public class VehiculoRepository : IVehiculosRepository
{
    /// <summary>
    /// Conexion entre la base de datos y el programa
    /// </summary>
    public AppDbContext _context;
    
    public VehiculoRepository(AppDbContext context)
    {
        _context = context;
    }
    
    /// <inheritdoc cref="GetAllAsync"/>
    public async Task<List<Vehiculo>> GetAllAsync()
    {
        return await _context.Vehiculos.Where(v=> v.Activo == true ).AsNoTracking().ToListAsync();
    }
    
    /// <inheritdoc cref="GetPaginatedVehiculosAsync"/>
    public async Task<(List<Vehiculo> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)>
        GetPaginatedVehiculosAsync
        (int pageNumber, int pageSize, CancellationToken ct = default)
    {
        // Obtener todos los registros disponibles
        var query = _context.Vehiculos.AsNoTracking().Where(v => v.Activo);

        // Calcular el total de registros
        int count = await query.CountAsync(ct);

        // Calcular el número total de páginas
        int totalPages = (int)Math.Ceiling((double)count / pageSize);

        // Obtener los vehiculos de la página solicitada
        List<Vehiculo> vehiculos = await query
            .OrderBy(f => f.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        bool hasPreviousPage = pageNumber > 1;
        bool hasNextPage = pageNumber < totalPages;

        return (vehiculos, count, totalPages, hasPreviousPage, hasNextPage);
    }

    /// <inheritdoc cref="GetByIdAsync"/>
    public async Task<Vehiculo?> GetByIdAsync(string id)
    {
        return await _context.Vehiculos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id && v.Activo == true);
    }

    /// <inheritdoc cref="GetByMatriculaAsync"/>
    public async Task<Vehiculo?> GetByMatriculaAsync(string matricula)
    {
        return await _context.Vehiculos.AsNoTracking().FirstOrDefaultAsync(v => v.Matricula == matricula && v.Activo == true);
    }

    /// <inheritdoc cref="CreateAsync"/>
    public async Task<Vehiculo> CreateAsync(Vehiculo obj)
    {
        obj.CreatedAt = DateTime.UtcNow;
        obj.UpdatedAt = DateTime.UtcNow;
        obj.Activo = true;
        obj.Id = null!;

        _context.Vehiculos.Add(obj);
        await _context.SaveChangesAsync();
        
        return obj;
    }

    /// <inheritdoc cref="UpdateAsync"/>
    public async Task<Vehiculo?> UpdateAsync(Vehiculo obj)
    {
        var response = await _context.Vehiculos.FirstOrDefaultAsync(v=> v.Id == obj.Id && v.Activo == true); 
        if (response == null) return null;

        obj.UpdatedAt = DateTime.Now;
        _context.Entry(response).CurrentValues.SetValues(obj);
        await _context.SaveChangesAsync();
        await _context.SaveChangesAsync();

        return response;
    }

    public async Task<Vehiculo?> UpdateEstado(string id, EstadoEnum estadoEnum)
    {
        var response = await _context.Vehiculos.FirstOrDefaultAsync(v=> v.Id == id && v.Activo == true); 
        if (response == null) return null;

        response.UpdatedAt = DateTime.Now;
        response.Estado = estadoEnum;
        
        _context.Entry(response).CurrentValues.SetValues(response);
        await _context.SaveChangesAsync();
        await _context.SaveChangesAsync();

        return response;
    }

    /// <inheritdoc cref="DeleteAsync"/>
    public async Task<bool> DeleteAsync(string id)
    {
        var objt = await _context.Vehiculos.FirstOrDefaultAsync(v=> v.Id == id && v.Activo == true); 
        if (objt == null) return false;

        objt.UpdatedAt = DateTime.UtcNow;
        objt.DeletedAt = DateTime.UtcNow;
        objt.Activo = false;
        objt.Estado = EstadoEnum.Retirado;
        
        await _context.SaveChangesAsync();

        return true;
    }
}