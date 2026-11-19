using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Repositories.Trabajadores;

public class TrabajadorRepository(AppDbContext db) : ITrabajadorRepository {
    
    // solo activos
    public async Task<List<Trabajador>> GetAllAsync() =>
        await db.Trabajadores.AsNoTracking()
            .Include(t => t.Usuario) // para tener email y tlf 
            .Where(t => t.IsActivo)
            .OrderBy(t => t.Apellidos)
            .ToListAsync();
    
    public async Task<(List<Trabajador>, int)> GetAllPagedAsync(int page, int pageSize, bool incluirInactivos = false, TipoTrabajador? tipo = null) {
        
        var query = db.Trabajadores
            .AsNoTracking()
            .Include(t => t.Usuario)
            .AsQueryable();

        // si no hay que incluir inactivos se añade la condicion
        if (!incluirInactivos) query = query.Where(t => t.IsActivo);
        // si se pide un tipo se añade la condición
        if (tipo is not null) query = query.Where(t => t.TipoTrabajador == tipo);

        var total = await query.CountAsync(); // se cuenta el num de trabajadores
        var items = await query
            .OrderBy(t => t.Apellidos)
            .ThenBy(t => t.Nombre)
            .Skip((page - 1) * pageSize) // formula mitica
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
    
    // devuelve inactivos
    public async Task<Trabajador?> GetByIdAsync(long id) =>
        await db.Trabajadores.AsNoTracking()
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.Id == id);
    
    public async Task<Trabajador?> GetByIdUsuarioAsync(long idUsuario) =>
        await db.Trabajadores.AsNoTracking()
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.IdUsuario == idUsuario);
    
    public async Task<Trabajador> CreateAsync(Trabajador trabajador) {
        db.Trabajadores.Add(trabajador);
        await db.SaveChangesAsync();
        return trabajador;
    }
    
    public async Task<Trabajador?> UpdateAsync(Trabajador trabajador) {
        var existente = await db.Trabajadores.FindAsync(trabajador.Id);
        if (existente is null) return null;

        existente.Nombre = trabajador.Nombre;
        existente.Apellidos = trabajador.Apellidos;
        existente.Dni = trabajador.Dni;
        existente.FechaIngreso = trabajador.FechaIngreso;
        existente.TipoTrabajador = trabajador.TipoTrabajador;
        existente.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return existente;
    }
    
    public async Task<bool> DeleteAsync(long id) {
        
        var t = await db.Trabajadores.FindAsync(id);
        if (t is null) return false; // si no lo encuentra

        t.IsActivo = false;
        t.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> ExistsByDniAsync(string dni) =>
        await db.Trabajadores.AnyAsync(t => t.Dni == dni);

    public async Task<bool> EsResponsableAsync(long idTrabajador) =>
        await db.Oficinas.AnyAsync(o => o.IdResponsable == idTrabajador);

    public async Task<bool> TieneReservasVivasAsync(long idUsuario) =>
        await db.Reservas
            .AnyAsync(r => r.TrabajadorId == idUsuario &&
                                        (r.Estado == Estado.Pendiente
                                         || r.Estado == Estado.Confirmada
                                         || r.Estado == Estado.EnCurso));
}