using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Dtos.Common;
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
    /// <param name="dto">Filtros y paginación añadidos</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve una lista de clientes con o sin filtros, con paginación incluida</returns>
    Task<Result<PagedResult<ClienteDto>, DomainError>> FindAllPagedAsync(ClienteFilterDto dto, CancellationToken cancellationToken);
    
    /// <summary>
    /// Busca un usuario específico
    /// </summary>
    /// <param name="id">Identificadoir del cliente</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuleve un cliente específico</returns>
    Task<Result<ClienteDto, DomainError>> GetByIdAsync(long id, CancellationToken cancellationToken);
    
    /// <summary>
    /// Actualiza tanto los datos de cliente commo de su usuario
    /// </summary>
    /// <param name="id">Identificador del cliente a actualizar</param>
    /// <param name="updatedCliente">Datos del cliente actualizados</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve los datos del cliente ta actualizados</returns>
    Task<Result<ClienteDto, DomainError>> UpdateAsync(long id, UpdateClienteDto updatedCliente, CancellationToken cancellationToken);

    /// <summary>
    /// Da de baja a un cliente en específico
    /// </summary>
    /// <param name="id">Identificador del cliente a dar de baja</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>No devuleve nada si ha salido todo correcto</returns>
    Task<UnitResult<DomainError>> DeleteAsync(long id, CancellationToken cancellationToken);
}