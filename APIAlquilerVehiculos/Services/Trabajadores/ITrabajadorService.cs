using APIAlquilerVehiculos.Dtos.Trabajadores;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Models.Usuarios;
using CSharpFunctionalExtensions;

namespace APIAlquilerVehiculos.Services.Trabajadores;

/// <summary>
/// 
/// </summary>
public interface ITrabajadorService {
    
    Task<Result<IReadOnlyList<ResponseTrabajadorDto>, DomainError>> GetAllPagedAsync(
        int page = 1, int pageSize = 10, bool incluirInactivos = false, TipoTrabajador? tipo = null);

    Task<Result<ResponseTrabajadorDto, DomainError>> GetByIdAsync(long id);

    Task<Result<ResponseTrabajadorDto, DomainError>> AddAsync(CreateTrabajadorDto dto);

    Task<Result<ResponseTrabajadorDto, DomainError>> UpdateAsync(long id, UpdateTrabajadorDto dto);

    Task<UnitResult<DomainError>> DeleteAsync(long id);
}