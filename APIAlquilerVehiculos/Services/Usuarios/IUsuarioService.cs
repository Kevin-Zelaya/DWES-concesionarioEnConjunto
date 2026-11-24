using System.Security.Claims;
using APIAlquilerVehiculos.Dtos.Usuarios;
using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

namespace APIAlquilerVehiculos.Services.Usuarios;

public interface IUsuarioService
{
    /// <summary>
    /// Buscar TODOS los clientes con filtros y paginación
    /// </summary>
    /// <param name="dto">Filtros y paginación añadidos</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve una lista de clientes con o sin filtros, con paginación incluida</returns>
    Task<Result<PaginatedResouce<UsuarioDto>, DomainError>> GetPaginatedUsuariosAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Busca un usuario específico
    /// </summary>
    /// <param name="id">Identificadoir del cliente</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuleve un cliente específico</returns>
    Task<Result<UsuarioDetalleDto, DomainError>> GetByIdAsync(long id, CancellationToken cancellationToken);
    
    /// <summary>
    /// Actualiza el estado del cliente, renovandolo
    /// </summary>
    /// <param name="id">Identificador del cliente a renovar</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve los datos del cliente ta actualizados</returns>
    //Task<Result<ClienteDto, DomainError>> UpdatePartialAsync(long id, CancellationToken cancellationToken);
    
    /// <summary>
    /// Actualiza el estado del cliente, renovandolo
    /// </summary>
    /// <param name="id">Identificador del cliente a renovar</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve los datos del cliente ta actualizados</returns>
    //Task<Result<ClienteDto, DomainError>> UpdatePartialAsync(long id, CancellationToken cancellationToken);

}