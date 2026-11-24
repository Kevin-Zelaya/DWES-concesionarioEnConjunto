using System.Security.Claims;
using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Dtos;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Models.Usuarios;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;

namespace APIAlquilerVehiculos.Services.Clientes;

public interface IClienteService
{
    /// <summary>
    /// Buscar TODOS los clientes con filtros y paginación
    /// </summary>
    /// <param name="page">Número de pagina</param>
    /// <param name="pageSize">Número de elementos por pagina</param>
    /// <param name="incluirInactivos">Incluir clientes inactivos</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve una lista de clientes con o sin filtros, con paginación incluida</returns>
    Task<Result<IReadOnlyList<ClienteDto>, DomainError>> FindAllPagedAsync(CancellationToken cancellationToken, int page = 1, int pageSize = 10,
        bool incluirInactivos = false);
    
    /// <summary>
    /// Busca un usuario específico
    /// </summary>
    /// <param name="id">Identificadoir del cliente</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <param name="principal">Contexto de la seguridad para buscar el usuario que inició sesión</param>
    /// <returns>Devuleve un cliente específico</returns>
    Task<Result<ClienteDto, DomainError>> GetByIdAsync(ClaimsPrincipal principal, long id, CancellationToken cancellationToken);
    
    /// <summary>
    /// Actualiza tanto los datos de cliente commo de su usuario
    /// </summary>
    /// <param name="id">Identificador del cliente a actualizar</param>
    /// <param name="updatedCliente">Datos del cliente actualizados</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <param name="principal">Contexto de la seguridad para buscar el usuario que inició sesión</param>
    /// <returns>Devuelve los datos del cliente ta actualizados</returns>
    Task<Result<ClienteDto, DomainError>> UpdateAsync(ClaimsPrincipal principal, long id, UpdateClienteDto updatedCliente, CancellationToken cancellationToken);

    /// <summary>
    /// Actualiza el estado del cliente, renovandolo
    /// </summary>
    /// <param name="id">Identificador del cliente a renovar</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve los datos del cliente ta actualizados</returns>
    Task<Result<ClienteDto, DomainError>> UpdatePartialAsync(long id, CancellationToken cancellationToken);
    
    /// <summary>
    /// Da de baja a un cliente en específico
    /// </summary>
    /// <param name="id">Identificador del cliente a dar de baja</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <param name="principal">Contexto de la seguridad para buscar el usuario que inició sesión</param>
    /// <returns>No devuleve nada si ha salido todo correcto</returns>
    Task<UnitResult<DomainError>> DeleteAsync(ClaimsPrincipal principal, long id, CancellationToken cancellationToken);
}