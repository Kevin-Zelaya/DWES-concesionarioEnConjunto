using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Repositories.Modelos;


/// <summary>
/// Repositorio de Modelo, acede a la base de datos desde el _context y gestiona la persistencia de datos
/// </summary>
public class ModeloRepository : IModeloRepository
{
    /// <summary>
    /// Conexion entre la base de datos y el programa
    /// </summary>
    public AppDbContext _context;
    
    public ModeloRepository(AppDbContext context)
    {
        _context = context;
    }
    
    /// <inheritdoc cref="GetAllAsync"/>
    public async Task<List<Modelo>> GetAllAsync()
    {
        return await _context.Modelos.Where(m=> m.Activo == true ).ToListAsync();
    }

    /// <inheritdoc cref="GetByIdAsync"/>
    public async Task<Modelo?> GetByIdAsync(int id)
    {
        return await _context.Modelos.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id && m.Activo == true);
    }
    
    /// <inheritdoc cref="GetPaginatedModeloAsync"/>
    public async Task<(List<Modelo> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)>
        GetPaginatedModeloAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    { 
        // Obtener todos los registros disponibles
        var query = _context.Modelos.AsNoTracking().Where(v => v.Activo);

        // Calcular el total de registros
        int count = await query.CountAsync(ct);

        // Calcular el número total de páginas
        int totalPages = (int)Math.Ceiling((double)count / pageSize);

        // Obtener los vehiculos de la página solicitada
        List<Modelo> modelos = await query
            .OrderBy(f => f.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        bool hasPreviousPage = pageNumber > 1;
        bool hasNextPage = pageNumber < totalPages;

        return (modelos, count, totalPages, hasPreviousPage, hasNextPage);
    }
    
    /// <inheritdoc cref="CreateAsync"/>
    public async Task<Modelo> CreateAsync(Modelo obj)
    {
        obj.CreatedAt = DateTime.UtcNow;
        obj.UpdatedAt = DateTime.UtcNow;
        obj.Activo = true;

        _context.Modelos.Add(obj);
        await _context.SaveChangesAsync();
        
        return obj;
    }
    
    /// <inheritdoc cref="UpdateAsync"/>
    public async Task<Modelo?> UpdateAsync(Modelo obj)
    {
        var response = await _context.Modelos.FirstOrDefaultAsync(m=> m.Id == obj.Id && m.Activo == true); 
        if (response == null) return null;

        obj.UpdatedAt = DateTime.Now;
        _context.Entry(response).CurrentValues.SetValues(obj);
        await _context.SaveChangesAsync();

        return response;
    }

    /// <inheritdoc cref="DeleteAsync"/>
    public async Task<bool> DeleteAsync(int id)
    {
        var objt = await _context.Modelos.FirstOrDefaultAsync(m=> m.Id == id && m.Activo == true); 
        if (objt == null) return false;

        objt.UpdatedAt = DateTime.UtcNow;
        objt.DeletedAt = DateTime.UtcNow;
        objt.Activo = false;
        
        await _context.SaveChangesAsync();

        return true;
    }
}