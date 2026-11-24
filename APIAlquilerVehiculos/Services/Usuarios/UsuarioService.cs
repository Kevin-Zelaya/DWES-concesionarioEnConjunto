using APIAlquilerVehiculos.Dtos.Usuarios;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Errors.Usuarios;
using APIAlquilerVehiculos.Models.Usuarios;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Services.Usuarios;

public class UsuarioService(ILogger<UsuarioService> logger, UserManager<Usuario> userManager,
    AppDbContext dbContext,
    ICache cache) : IUsuarioService
{
    
    private static string CacheKey(long id) => $"usuario:{id}";

    
    /// <summary>
    /// Buscar usuarios por paginación y filtros opcionales
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <param name="pageNumber">Número de pagina</param>
    /// <param name="pageSize">Número de elemetos por pagina</param>
    /// <returns></returns>
    public async Task<Result<PaginatedResouce<UsuarioDto>, DomainError>> GetPaginatedUsuariosAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        //Validamos paginacion
        var page = pageNumber < 1 ? 1 : pageNumber;
        var size = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

        logger.LogDebug("[SERVICE-GETPAGINATED] Recuperando usuarios. Página {PageNumber}, tamaño {PageSize}.", page, size);

        // Para buscar usuarios que tambien esten dados e baja
        var baseQuery = userManager.Users
            .AsNoTracking()
            .IgnoreQueryFilters();

        // total de usuarios
        var count = await baseQuery.CountAsync(cancellationToken);

        //
        var rawItems = await baseQuery
            .OrderBy(u => u.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(u => new
            {
                Usuario = u,
                Nombre = u.Cliente != null
                    ? u.Cliente.Nombre
                    : (u.Trabajador != null ? u.Trabajador.Nombre : string.Empty),
                Apellidos = u.Cliente != null
                    ? u.Cliente.Apellidos
                    : (u.Trabajador != null ? u.Trabajador.Apellidos : string.Empty),
                TipoUsuario = u.Cliente != null
                    ? "Cliente"
                    : (u.Trabajador != null ? "Trabajador" : "Usuario"),
                IsActivo = u.Cliente != null
                    ? u.Cliente.IsActivo
                    : (u.Trabajador != null ? u.Trabajador.IsActivo : true),
                Rol = (
                    from ur in dbContext.UserRoles
                    join r in dbContext.Roles on ur.RoleId equals r.Id
                    where ur.UserId == u.Id
                    select r.Name
                ).FirstOrDefault() ?? string.Empty
            })
            .ToListAsync(cancellationToken);
        
        //Mapeamos respuesta
        var items = rawItems
            .Select(x => new UsuarioDto(
                x.Usuario.Id,
                x.Usuario.Email ?? string.Empty,
                x.Nombre,
                x.Apellidos,
                x.Usuario.PhoneNumber,
                x.Rol,
                x.TipoUsuario,
                x.IsActivo,
                x.Usuario.CreatedAt))
            .ToList();

        // Calculo de páginas en total.
        var totalPages = count == 0 ? 0 : (int)Math.Ceiling(count / (double)size);
        // True solo si estás en una página posterior a la 1 y existen páginas.
        var hasPreviousPage = page > 1 && totalPages > 0;
        // True si la página actual es menor que el total de páginas.
        var hasNextPage = page < totalPages;

        var response = new PaginatedResouce<UsuarioDto>(
            items, count, totalPages, hasPreviousPage, hasNextPage);

        return Result.Success<PaginatedResouce<UsuarioDto>, DomainError>(response)
            .Tap(r => logger.LogInformation(
                "[SERVICE-GETPAGINATED] Se recuperaron {Count} usuarios de la página {PageNumber}.", r.Items.Count, page));
    }

    /// <summary>
    ///  Buscar un usuario concreto
    /// </summary>
    /// <param name="id">Identificador del usuario</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns></returns>
    public async Task<Result<UsuarioDetalleDto, DomainError>> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Buscando usuario {Id}", id);

        if (id <= 0)
        {
            logger.LogError("El ID {Id} no se ha encontrado", id);
            return Result.Failure<UsuarioDetalleDto, DomainError>(UsuarioErrors.NotFound(id));
        }
        
        var cached = await cache.GetTAsync<UsuarioDetalleDto>(CacheKey(id));
        if (cached is not null) {
            logger.LogDebug("Usuario {Id} obtenido de la caché", id);
            return Result.Success<UsuarioDetalleDto, DomainError>(cached);
        }
        
        var user = await dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(u => u.Cliente)
            .Include(u => u.Trabajador)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("Usuario {Id} NO encontrado", id);
            return Result.Failure<UsuarioDetalleDto, DomainError>(UsuarioErrors.NotFound(id));
        }

        
        var rol = await (
            from ur in dbContext.UserRoles
            join r in dbContext.Roles on ur.RoleId equals r.Id
            where ur.UserId == user.Id
            select r.Name
        ).FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        string nombreYApellidos;
        string tipoUsuario;
        bool isActivo;
        DateTime? updatedAt;

        if (user.Cliente is not null)
        {
            nombreYApellidos = $"{user.Cliente.Nombre} {user.Cliente.Apellidos}".Trim();
            tipoUsuario = "Cliente";
            isActivo = user.Cliente.IsActivo;
            updatedAt = user.Cliente.UpdatedAt;
        }
        else if (user.Trabajador is not null)
        {
            nombreYApellidos = $"{user.Trabajador.Nombre} {user.Trabajador.Apellidos}".Trim();
            tipoUsuario = "Trabajador";
            isActivo = user.Trabajador.IsActivo;
            updatedAt = user.Trabajador.UpdatedAt;
        }
        else
        {
            nombreYApellidos = "Admin";
            tipoUsuario = "Admin";
            isActivo = true;
            updatedAt = null;
        }

        var response = new UsuarioDetalleDto(
            Id: user.Id,
            Email: user.Email ?? string.Empty,
            NombreYApellidos: nombreYApellidos,
            Telefono: user.PhoneNumber,
            Rol: rol,
            TipoUsuario: tipoUsuario,
            IsActivo: isActivo,
            CreatedAt: user.CreatedAt,
            UpdatedAt: updatedAt
        );

        await cache.SetAsync(CacheKey(id), response);

        return Result.Success<UsuarioDetalleDto, DomainError>(response);
    }
    
}