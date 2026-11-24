using System.Linq.Expressions;
using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Repositories.Clientes;

public class ClienteRepository(AppDbContext db) : IClienteRepository
{
    #region CRUD

    /// <summary>
    ///  Busqueda de clientes con paginado y filtros de busqueda
    /// </summary>
    /// <param name="dto">Filtros y paginación añadida</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve una lista de clientes</returns>
    public async Task<(List<Cliente> Items, int TotalCount)> FindAllPagedAsync(ClienteFilterDto dto, CancellationToken cancellationToken = default)
    {
        var query = dto.IsActivo.HasValue
            ? db.Clientes.IgnoreQueryFilters().Include(c => c.Usuario).AsQueryable()
            : db.Clientes.Include(c => c.Usuario).AsQueryable();

        if (!string.IsNullOrWhiteSpace(dto.Nombre))
            query = query.Where(f => EF.Functions.Like(f.Nombre, $"%{dto.Nombre}%"));

        if (!string.IsNullOrWhiteSpace(dto.Apellidos))
            query = query.Where(f => EF.Functions.Like(f.Apellidos, $"%{dto.Apellidos}%"));
        
        if (!string.IsNullOrWhiteSpace(dto.Email))
            query = query.Where(p => EF.Functions.Like(p.Usuario.Email, $"%{dto.Email}%"));

        if (dto.IsActivo.HasValue)
            query = query.Where(p => p.IsActivo == dto.IsActivo.Value);
        
        
        var totalCount = await query.CountAsync(cancellationToken);
        query = ApplySorting(query, dto.SortBy, dto.Direction);
        
        var size = Math.Clamp(dto.Size, 1, 100);
        var page = Math.Max(dto.Page, 0);

        var items = await query
            .AsNoTracking().Skip(page * size).Take(size).ToListAsync(cancellationToken);

        return (items, totalCount);
    }
    

    /// <summary>
    ///  Buscar un cliente concreto con su identificador
    ///  Con acceso el Administrador y el propio cliente
    /// </summary>
    /// <param name="id">Identificador del cliente</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve los datos especificos del cliente</returns>
    public async Task<Cliente?> FindByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var cliente = await db.Clientes.IgnoreQueryFilters().AsNoTracking().Include(c => c.Usuario).FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        return cliente;
    }
    
    /// <summary>
    ///  Buscar un cliente concreto con su identificador
    ///  Con acceso el Administrador y el propio cliente
    ///  mediante el id de su usuario
    /// </summary>
    /// <param name="id">Identificador del usuario del cliente</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve los datos especificos del cliente</returns>
    public async Task<Cliente?> FindByUserId(long id, CancellationToken cancellationToken = default)
    {
        var cliente = await db.Clientes.AsNoTracking().FirstOrDefaultAsync(f => f.IdUsuario == id, cancellationToken);
        return cliente;
    }

    // La creación del cliente es donde el signup de Auth
    // Si pusieramos Post de Add cliente duplicariamos funcionalidad
    // Alo mejor se podria llegar a poer para un trabajador cliente, pero con el registro es más que suficiente.

    /// <summary>
    /// Actualizar datos del cliente
    /// Con acceso el Administrador y el propio cliente
    /// </summary>
    /// <param name="id">Identificador del cliente</param>
    /// <param name="updatedCliente">Datos actualizados del cliente</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns> Devuelve el cliente actualizado</returns>
    public async Task<Cliente?> UpdateAsync(long id, Cliente updatedCliente, CancellationToken cancellationToken = default)
    {
        var cliente = await db.Clientes.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        if (cliente is null) return null;
        
        cliente.Nombre = updatedCliente.Nombre;
        cliente.Apellidos = updatedCliente.Apellidos;
        cliente.FechaNacimiento = updatedCliente.FechaNacimiento;
        cliente.Dni = updatedCliente.Dni;
        cliente.Nie = updatedCliente.Nie;
        cliente.Pasaporte = updatedCliente.Pasaporte;
        cliente.NumeroPermisoConducir = updatedCliente.NumeroPermisoConducir;
        cliente.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync(cancellationToken);
        
        await db.Entry(cliente).ReloadAsync(cancellationToken);
        
        return cliente;
    }
    
    /// <summary>
    /// Actualizar estado del cliente de baja a activo
    /// Acceso único por el Administrador
    /// </summary>
    /// <param name="cliente">cliente actual</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve el cliente renovado</returns>
    public async Task<Cliente?> UpdatePartialAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        cliente.IsActivo = true;
        cliente.DeletedAt = null;
        cliente.UpdatedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync(cancellationToken);
        
        return cliente;
    }
    
    /// <summary>
    /// Dar de baja logica un cliente
    /// Y este no debe poder aparecer en niguna otra acción, ni debe poder chocar valores con otros usuarios
    /// </summary>
    /// <param name="id">Identificador del cliente</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve verdadero (true) si se ha eliminado</returns>
    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (cliente is null) return false;

        cliente.IsActivo = false;
        cliente.DeletedAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync(cancellationToken);
        
        // Refresca los valores desde la BD para asegurar que se trae la versión exacta guardada
        await db.Entry(cliente).ReloadAsync(cancellationToken);
        
        return true;
    }
    #endregion

    #region Consultas adicionales

    
    /// <summary>
    ///  Comprobar si ya existe un cliente con un perfil usuario
    /// </summary>
    /// <param name="id">Identificador del usuario</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve true si si existe</returns>
    public async Task<bool> ExistsClientesWithIdUserAsync(long id, CancellationToken cancellationToken = default)
    {
        return await db.Clientes.AnyAsync(f => f.IdUsuario == id, cancellationToken);
    }
    
    /// <summary>
    /// Comprobar si el cliente tienen alguna reserva viva (Pendiente, Confirmada o EnCurso)
    /// </summary>
    /// <param name="idCliente">Identificador del cliente</param>
    /// <returns>Devuelve true si tiene reservas aún vivas</returns>
    public async Task<bool> TieneReservasVivasAsync(long idCliente) =>
        await db.Reservas
            .AnyAsync(r => r.ClienteId == idCliente  &&
                           (r.Estado == Estado.Pendiente
                            || r.Estado == Estado.Confirmada
                            || r.Estado == Estado.EnCurso));

    /// <summary>
    /// Ordenación para consultar los clientes según el campo y la dirección indicados.
    /// </summary>
    /// <param name="query">Consulta de clientes para usar la ordenación (getallpaged).</param>
    /// <param name="sortBy">Campo por el que se ordenarán los clientes (nombre, apellidos, email o id).</param>
    /// <param name="direction">Dirección de la ordenación: "asc" para ascendente o "desc" para descendente.</param>
    /// <returns>Consulta de clientes ordenada según los criterios especificados.</returns>
    private static IQueryable<Cliente> ApplySorting(IQueryable<Cliente> query, string sortBy, string direction)
    {
        var isDescending = direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        Expression<Func<Cliente, object>> keySelector = sortBy.ToLower() switch
        {
            "nombre" => f => f.Nombre,
            "apellidos" => f => f.Apellidos,
            "email" => f => f.IdUsuario,
            _ => f => f.Id
        };
        return isDescending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }

    #endregion
}