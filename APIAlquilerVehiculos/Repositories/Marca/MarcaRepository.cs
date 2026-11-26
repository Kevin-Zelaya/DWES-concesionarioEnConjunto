using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Repositories.Marcas;

/// <summary>
/// Repositorio de Marcas. Accede a la base de datos desde
/// el _context y gestiona la persistencia de datos.
/// </summary>
public class MarcaRepository : IMarcaRepository
{
    /// <summary>
    /// Conexión entre la base de datos y el programa.
    /// </summary>
    public AppDbContext _context;

    public MarcaRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc cref="GetAllAsync"/>
    public async Task<List<Marca>> GetAllAsync()
    {
        return await _context.Marcas
            .Where(m => m.Activo)
            .OrderBy(m => m.Id)
            .ToListAsync();
    }

    /// <inheritdoc cref="GetPaginatedMarcasAsync"/>
    public async Task<(List<Marca> items, int count, int totalPages,
        bool HasPreviousPage, bool HasNextPage)>
        GetPaginatedMarcasAsync(int pageNumber, int pageSize)
    {
        // Comprobar que los parámetros de paginación sean válidos
        if (pageNumber < 1 || pageSize < 1)
            throw new ArgumentException(
                "Los parámetros de paginación deben ser mayores que 0.");

        // Obtener todos los registros disponibles
        var query = _context.Marcas
            .AsNoTracking()
            .Where(m => m.Activo);

        // Calcular el total de registros
        int count = await query.CountAsync();

        // Calcular el número total de páginas
        int totalPages = (int)Math.Ceiling(
            (double)count / pageSize);

        // Obtener las marcas de la página solicitada
        List<Marca> marcas = await query
            .OrderBy(m => m.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        bool hasPreviousPage = pageNumber > 1;
        bool hasNextPage = pageNumber < totalPages;

        return (marcas, count, totalPages,
            hasPreviousPage, hasNextPage);
    }

    /// <inheritdoc cref="GetByIdAsync"/>
    public async Task<Marca?> GetByIdAsync(long id)
    {
        return await _context.Marcas
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.Id == id && m.Activo);
    }

    /// <inheritdoc cref="GetByNombreAsync"/>
    public async Task<Marca?> GetByNombreAsync(string nombre)
    {
        return await _context.Marcas
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.Nombre == nombre && m.Activo);
    }

    /// <inheritdoc cref="CreateAsync"/>
    public async Task<Marca> CreateAsync(Marca obj)
    {
        obj.CreatedAt = DateTime.UtcNow;
        obj.UpdatedAt = DateTime.UtcNow;
        obj.DeletedAt = null;
        obj.Activo = true;

        _context.Marcas.Add(obj);
        await _context.SaveChangesAsync();

        return obj;
    }

    /// <inheritdoc cref="UpdateAsync"/>
    public async Task<Marca?> UpdateAsync(Marca obj)
    {
        var response = await _context.Marcas
            .FirstOrDefaultAsync(
                m => m.Id == obj.Id && m.Activo);

        if (response == null)
            return null;

        response.Nombre = obj.Nombre;
        response.Pais = obj.Pais;
        response.Wb = obj.Wb;

        response.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return response;
    }

    /// <inheritdoc cref="DeleteAsync"/>
    public async Task<bool> DeleteAsync(long id)
    {
        var obj = await _context.Marcas
            .FirstOrDefaultAsync(
                m => m.Id == id && m.Activo);

        if (obj == null)
            return false;

        obj.UpdatedAt = DateTime.UtcNow;
        obj.DeletedAt = DateTime.UtcNow;
        obj.Activo = false;

        await _context.SaveChangesAsync();

        return true;
    }
}