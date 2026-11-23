using System.ComponentModel.DataAnnotations;
using System.Net;
using APIAlquilerVehiculos.DTOs.Marcas;
using APIAlquilerVehiculos.Dtos.Vehiculo;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Mappers.Vehiculos;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Repositories.Marcas;
using APIAlquilerVehiculos.Repositories.Vehiculos;
using CSharpFunctionalExtensions;
using FluentValidation;

namespace APIAlquilerVehiculos.Services.Vehiculo;

public class VehiculoService : IVehiculoService
{
    private ICache _cache;
    private IVehiculosRepository _vehiculosRepository;
    private IValidator<VehiculoCreateDto> _validatorCreate;
    // cambiar por reservas y añadir partes
    private IMarcaRepository _marcaRepository;
    
    public VehiculoService(ICache cache, IVehiculosRepository vehiculosRepository, IValidator<VehiculoCreateDto> validatorCreate, IMarcaRepository marcaRepository)
    {
        _cache = cache;
        _vehiculosRepository = vehiculosRepository;
        _validatorCreate = validatorCreate;
        // cambiar por reservas y añadir partes
        _marcaRepository = marcaRepository;
    }
    
    /// <inheritdoc cref="GetAllAsync"/>
    public async Task<Result<List<VehiculoResponseDto>, DomainError>> GetAllAsync(CancellationToken ct = default)
    {
        //Buscamos en el reositorio los datos
        var all = await _vehiculosRepository.GetAllAsync();
        //Los pasamos a dtos de respuesta 
        var all3 = all.Select(v => VehiculoMapper.ToResponseDto(v)).ToList();
        //Y los devolvemos
        return Result.Success<List<VehiculoResponseDto>, DomainError>(all3);
    }

    /// <inheritdoc cref="GetByIdAsync"/>
    public async Task<Result<VehiculoResponseDto, DomainError>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        // Buscamos en la caché, si no se encuentra se busca en el repositorio
        var rest = await _cache.GetTAsync<Models.Vehiculo>(id) ?? await _vehiculosRepository.GetByIdAsync(id);

        // Almacenamos en la cache en caso de no ser nulo
        if (rest != null) await _cache.SetAsync(rest.Id,rest);
        
        // Devolvemos un erro en caso de no encontrarlo y el vehículo en caso de encontrarlo 
        return rest == null
            ? Result.Failure<VehiculoResponseDto, DomainError>(NotFoundError.FromResource("vehiculo", $"{id}"))
            : Result.Success<VehiculoResponseDto, DomainError>(VehiculoMapper.ToResponseDto(rest));
    }

    /// <inheritdoc cref="GetByMatriculaAsync"/>
    public async Task<Result<VehiculoResponseDto,DomainError>> GetByMatriculaAsync
    (string matricula, CancellationToken ct = default)
    {
        // Buscamos en la caché, si no se encuentra se busca en el repositorio
        var rest = await _vehiculosRepository.GetByMatriculaAsync(matricula);
        
        // Almacenamos en la cache en caso de no ser nulo
        if (rest != null) await _cache.SetAsync(rest.Matricula,rest);
        
        // Devolvemos un erro en caso de no encontrarlo y el vehículo en caso de encontrarlo 
        return rest == null
            ? Result.Failure<VehiculoResponseDto, DomainError>(NotFoundError.Messagee($"No existe un vehiculo con la mtricula{matricula}"))
            : Result.Success<VehiculoResponseDto, DomainError>(VehiculoMapper.ToResponseDto(rest));
    }

    /// <inheritdoc cref="CreateAsync"/>
    public async Task<Result<VehiculoResponseDto, DomainError>> CreateAsync(VehiculoCreateDto dto, CancellationToken ct = default)
    {
        var existMatricula = await _vehiculosRepository.GetByMatriculaAsync(dto.Matricula); 
        if (existMatricula != null)
            return Result.Failure<VehiculoResponseDto, DomainError>(NotFoundError.Messagee($"Ya existe un vehiculo con la matricula {dto.Matricula}"));

        var vehiculo = await _vehiculosRepository.CreateAsync(VehiculoMapper.ToVehiculo(dto));
        var response = VehiculoMapper.ToResponseDto(vehiculo);
        return Result.Success<VehiculoResponseDto, DomainError>(response);
    }

    /// <inheritdoc cref="UpdateAsync"/>
    public async Task<Result<VehiculoResponseDto, DomainError>> UpdateAsync(string id, VehiculoUpdateDto dto, CancellationToken ct = default)
    {
        //conflicto por matrricula repetida
        var existMatricula = await _vehiculosRepository.GetByMatriculaAsync(dto.Matricula); 
        if (existMatricula != null)
            return Result.Failure<VehiculoResponseDto, DomainError>(NotFoundError.Messagee($"Ya existe un vehiculo con la matricula {dto.Matricula}"));

        //conflicto por la falta de existencia del objeto a actualizar
        var exist = await _vehiculosRepository.GetByIdAsync(id);
        if (exist == null)
            return Result.Failure<VehiculoResponseDto, DomainError>(NotFoundError.FromResource("vehiculo", $"{id}"));
        
        // si se cambia el estado a retirado se cambia el activo a false
        if (dto.Estado == EstadoEnum.Retirado)
            dto.Activo = false;
        
        // no se puede poner el coche como disponible si tiene partes activos o esta siendo alquilado
        
        // no se puede modificar el estado a retirado con partes abiertos o una reserva vigente 
        
        // no se puede cambiar el estado a en manteniemineto si tiene reservas activas
        
        // no se puede poner en alquiler si toiene partes sin resolver o ya esta siendo alquilado
        
        // no se puede reservar si esta con partes abiertos
        
        var vehiculo = VehiculoMapper.ToVehiculo(dto);
        vehiculo.Id = id;

        await _vehiculosRepository.UpdateAsync(vehiculo);
        
        await _cache.SetAsync(id, vehiculo);
        
        return Result.Success<VehiculoResponseDto, DomainError>(VehiculoMapper.ToResponseDto(vehiculo));
    }

    /// <inheritdoc cref="DeleteAsync"/>
    public async Task<UnitResult<DomainError>> DeleteAsync(string id, CancellationToken ct = default)
    {
        // no eliminar en caso de que no exista
        var exist = await _vehiculosRepository.GetByIdAsync(id);
        if (exist == null)
            return UnitResult.Failure<DomainError>(NotFoundError.FromResource("vehiculo", $"{id}"));
        // no eliminar en caso de que no este en estado disponible
        if (exist.Estado != EstadoEnum.Disponible )
            return UnitResult.Failure<DomainError>(ConflictError.Business($"No se puede eliminar un vehiculo que no este en estado Disponible"));
        // no eliminar en caso de que existan reservas de este vehiculo
        var marcaExist = await _marcaRepository.GetByIdAsync(exist.Marca_id);
        if (marcaExist != null)
            return UnitResult.Failure<DomainError>(NotFoundError.Messagee($"No se puede eliminar"));

        // no eliminar en caso de que existan partes activos de este vehiculo
        
        var deletd = await _vehiculosRepository.DeleteAsync(id);

        if (deletd == false)
            return UnitResult.Failure<DomainError>(NotFoundError.FromResource("vehiculo", $"{id}"));

        return UnitResult.Success<DomainError>();
    }
    
    /// <inheritdoc cref="GetPaginatedVehiculosAsync"/>
    public async Task<Result<(List<VehiculoResponseDto> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage), DomainError>> GetPaginatedVehiculosAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        // Comprobamos que los parámetros de paginación sean válidos.
        // Tanto el número de página como el tamaño deben ser mayores que 0.
        if (pageNumber < 1 || pageSize < 1)
            return Result.Failure<
                (List<VehiculoResponseDto> items, int count, int totalPages,
                bool HasPreviousPage, bool HasNextPage), DomainError>(
                new DomainError(
                    "Los parámetros de paginación no son válidos.",
                    HttpStatusCode.BadRequest));

        // Solicitamos al repositorio los vehiculos correspondientes a esa página.
        var resultado = await _vehiculosRepository.GetPaginatedVehiculosAsync(
            pageNumber, pageSize, ct);

        // Convertimos los vehiculos obtenidas a DTOs.
        var vehiculosDto = resultado.items.Select(m => VehiculoMapper.ToResponseDto(m)).ToList();

        // Devolvemos las marcas junto con la información de paginación.
        return Result.Success<
            (List<VehiculoResponseDto> items, int count, int totalPages,
            bool HasPreviousPage, bool HasNextPage), DomainError>(
            (vehiculosDto, resultado.count, resultado.totalPages,
                resultado.HasPreviousPage, resultado.HasNextPage));
    }
}