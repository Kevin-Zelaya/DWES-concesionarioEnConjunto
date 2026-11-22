using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Dtos.Common;
using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

namespace APIAlquilerVehiculos.Services.Clientes;

public interface IClienteService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<Result<PagedResult<ClienteDto>, DomainError>> FindAllPagedAsync(ClienteFilterDto dto);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Result<ClienteDto, DomainError>> GetByIdAsync(long id);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updatedFunko"></param>
    /// <returns></returns>
    Task<Result<ClienteDto, DomainError>> UpdateAsync(long id, ClienteDto updatedFunko);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Result<bool, DomainError>> DeleteAsync(long id);
}