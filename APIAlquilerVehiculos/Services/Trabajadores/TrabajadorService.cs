using APIAlquilerVehiculos.Dtos.Trabajadores;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Errors.Trabajador;
using APIAlquilerVehiculos.Mappers;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Repositories.Trabajadores;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Services.Trabajadores;

public class TrabajadorService(
    ITrabajadorRepository trabajadorRepository,
    IValidator<CreateTrabajadorDto> createValidator,
    IValidator<UpdateTrabajadorDto> updateValidator,
    UserManager<Usuario> userManager,
    AppDbContext dbContext,
    ICache cache,
    ILogger<TrabajadorService> log) 
    : ITrabajadorService {
    
    private static string CacheKey(long id) => $"trabajador:{id}";
    
    public async Task<Result<IReadOnlyList<ResponseTrabajadorDto>, DomainError>> GetAllPagedAsync(
        int page = 1, int pageSize = 10, bool incluirInactivos = false, TipoTrabajador? tipo = null) {
        
        log.LogInformation("Obteniendo trabajadores: página {Page}, tamaño {PageSize}", page, pageSize);
        
        // evita skip negativos o páginas gigantes
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        
        var (items, _) = await trabajadorRepository.GetAllPagedAsync(page, pageSize, incluirInactivos, tipo);
        var dtos = items.Select(t => t.ToResponseDto()).ToList();

        return Result.Success<IReadOnlyList<ResponseTrabajadorDto>, DomainError>(dtos);
    }

    public async Task<Result<ResponseTrabajadorDto, DomainError>> GetByIdAsync(long id) {
        
        log.LogInformation("Obteniendo trabajador {Id}", id);
        
        // cache-aside, primeor la caché
        var cached = await cache.GetTAsync<ResponseTrabajadorDto>(CacheKey(id));
        if (cached is not null) {
            log.LogDebug("Trabajador {Id} obtenido de la caché", id);
            return Result.Success<ResponseTrabajadorDto, DomainError>(cached);
        }
        
        // si no está, a la BD
        var trabajador = await trabajadorRepository.GetByIdAsync(id);
        if (trabajador is null) {
            log.LogWarning("Trabajador {Id} no encontrado", id);
            return Result.Failure<ResponseTrabajadorDto, DomainError>(TrabajadorErrors.NotFound(id));
        } 
        
        var respuesta = trabajador.ToResponseDto();
        await cache.SetAsync(CacheKey(id), respuesta);
        return Result.Success<ResponseTrabajadorDto, DomainError>(respuesta);
    }

    public async Task<Result<ResponseTrabajadorDto, DomainError>> AddAsync(CreateTrabajadorDto dto) {
        
        log.LogInformation("Creando trabajador {Email}", dto.Email);
        
        // validar formato
        var validationResult = await createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid) {
            return Result.Failure<ResponseTrabajadorDto, DomainError>(
                TrabajadorErrors.Validation(validationResult.ToDictionary()));
        }

        // comprueba que no exista con ese dni ni email
        var dniNorm = dto.Dni.Trim().ToUpperInvariant();
        if (await trabajadorRepository.ExistsByDniAsync(dniNorm))
            return Result.Failure<ResponseTrabajadorDto, DomainError>(TrabajadorErrors.DuplicateDni(dniNorm));

        if (await userManager.FindByEmailAsync(dto.Email) is not null)
            return Result.Failure<ResponseTrabajadorDto, DomainError>(TrabajadorErrors.DuplicateEmail(dto.Email));

        // usuario + rol + trabajador
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var user = new Usuario
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.Telefono,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, dto.Contraseña);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                return Result.Failure<ResponseTrabajadorDto, DomainError>(
                    TrabajadorErrors.IdentityError(result.Errors.Select(e => e.Description)));
            }

            var rol = dto.TipoTrabajador.ToRol();
            var roleResult = await userManager.AddToRoleAsync(user, rol);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                log.LogError("No se pudo asignar el rol {Rol} al usuario {UserId}", rol, user.Id);
                return Result.Failure<ResponseTrabajadorDto, DomainError>(
                    TrabajadorErrors.ErrorServer("Error interno al asignar los permisos del trabajador."));
            }

            var creado = await trabajadorRepository.CreateAsync(dto.ToEntity(user));
            await transaction.CommitAsync();

            await cache.SetAsync(CacheKey(creado.Id), creado.ToResponseDto());
            log.LogInformation("Trabajador {Id} creado", creado.Id);
            return Result.Success<ResponseTrabajadorDto, DomainError>(creado.ToResponseDto());
        }
        catch (DbUpdateException ex)
        {
            // carrera
            await transaction.RollbackAsync();
            log.LogWarning(ex, "Conflicto de unicidad al crear trabajador");
            return Result.Failure<ResponseTrabajadorDto, DomainError>(TrabajadorErrors.DuplicateDniOrEmail());
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            log.LogError(ex, "Error inesperado al crear el trabajador");
            return Result.Failure<ResponseTrabajadorDto, DomainError>(
                TrabajadorErrors.ErrorServer("Error inesperado al crear el trabajador"));
        }
    }

    public async Task<Result<ResponseTrabajadorDto, DomainError>> UpdateAsync(long id, UpdateTrabajadorDto dto) {
        
        // validar formato
        var validationResult = await updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid) 
            return Result.Failure<ResponseTrabajadorDto, DomainError>(
                TrabajadorErrors.Validation(validationResult.ToDictionary()));

        // existencia
        var actual = await trabajadorRepository.GetByIdAsync(id);
        if (actual is null)
            return Result.Failure<ResponseTrabajadorDto, DomainError>(TrabajadorErrors.NotFound(id));

        // unicidad solo si el valor cambia
        var dniNorm = dto.Dni.Trim().ToUpperInvariant();
        if (dniNorm != actual.Dni && await trabajadorRepository.ExistsByDniAsync(dniNorm))
            return Result.Failure<ResponseTrabajadorDto, DomainError>(TrabajadorErrors.DuplicateDni(dniNorm));

        var otroConEmail = await userManager.FindByEmailAsync(dto.Email);
        if (otroConEmail is not null && otroConEmail.Id != actual.IdUsuario)
            return Result.Failure<ResponseTrabajadorDto, DomainError>(TrabajadorErrors.DuplicateEmail(dto.Email));

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            // datos propios del trabajador
            var actualizado = await trabajadorRepository.UpdateAsync(dto.UpdateEntity(id));
            if (actualizado is null) {
                await transaction.RollbackAsync();
                return Result.Failure<ResponseTrabajadorDto, DomainError>(TrabajadorErrors.NotFound(id));
            }

            // email, teléfono y rol viven en Usuario / Identity
            var identityResult = await SincronizarUsuarioAsync(actual, dto);
            if (!identityResult.Succeeded) {
                await transaction.RollbackAsync();
                return Result.Failure<ResponseTrabajadorDto, DomainError>(
                    TrabajadorErrors.IdentityError(identityResult.Errors.Select(e => e.Description)));
            }

            await transaction.CommitAsync();

            await cache.RemoveAsync(CacheKey(id));

            // se vuelve a leer para que la respuesta lleve Usuario (email y teléfono) actualizado
            var recargado = await trabajadorRepository.GetByIdAsync(id);
            return Result.Success<ResponseTrabajadorDto, DomainError>(recargado!.ToResponseDto());
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            return Result.Failure<ResponseTrabajadorDto, DomainError>(TrabajadorErrors.DuplicateDniOrEmail());
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result.Failure<ResponseTrabajadorDto, DomainError>(
                TrabajadorErrors.ErrorServer("Error inesperado al modificar el trabajador"));
        }
    }

    public async Task<UnitResult<DomainError>> DeleteAsync(long id) {
        
        log.LogInformation("Dando de baja al trabajador {Id}", id);
        
        // se comprueba que existe
        var existente = await trabajadorRepository.GetByIdAsync(id);
        if (existente is null) {
            log.LogWarning("Trabajador {Id} no encontrado al dar de baja", id);
            return UnitResult.Failure<DomainError>(TrabajadorErrors.NotFound(id));
        }

        // se comprueba que está activo
        if (!existente.IsActivo)
            return UnitResult.Failure<DomainError>(TrabajadorErrors.YaInactivo(id));

        // comprueba si es responsable de oficina
        if (await trabajadorRepository.EsResponsableAsync(existente.Id))
            return UnitResult.Failure<DomainError>(TrabajadorErrors.EsResponsable(id));

        // comprueba si tiene reservas vivas
        if (await trabajadorRepository.TieneReservasVivasAsync(existente.Id))
            return UnitResult.Failure<DomainError>(TrabajadorErrors.TieneReservasVivas(id));

        // borrado lógico
        var borrado = await trabajadorRepository.DeleteAsync(id);
        
        // borra de caché
        await cache.RemoveAsync(CacheKey(id));
        
        log.LogInformation("Trabajador {Id} dado de baja", id);
        return !borrado ? UnitResult.Failure<DomainError>(TrabajadorErrors.NotFound(id)) :
            UnitResult.Success<DomainError>();
    }
    
    /// <summary>
    /// Actualiza email, teléfono y rol del Usuario asociado. Solo toca lo que ha cambiado.
    /// Del rol solo cambia el de tipo (Operario/Mecanico); un Responsable conserva ese rol.
    /// </summary>
    private async Task<IdentityResult> SincronizarUsuarioAsync(Trabajador actual, UpdateTrabajadorDto dto)
    {
        var user = await userManager.FindByIdAsync(actual.IdUsuario.ToString());
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "No existe el usuario asociado al trabajador" });

        IdentityResult r;

        if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            r = await userManager.SetEmailAsync(user, dto.Email);
            if (!r.Succeeded) return r;
            r = await userManager.SetUserNameAsync(user, dto.Email); // el username es el email
            if (!r.Succeeded) return r;
        }

        if (user.PhoneNumber != dto.Telefono)
        {
            r = await userManager.SetPhoneNumberAsync(user, dto.Telefono);
            if (!r.Succeeded) return r;
        }

        if (actual.TipoTrabajador != dto.TipoTrabajador)
        {
            var rolViejo = actual.TipoTrabajador.ToRol();
            var rolNuevo = dto.TipoTrabajador.ToRol();

            if (await userManager.IsInRoleAsync(user, rolViejo))
            {
                r = await userManager.RemoveFromRoleAsync(user, rolViejo);
                if (!r.Succeeded) return r;
            }

            r = await userManager.AddToRoleAsync(user, rolNuevo);
            if (!r.Succeeded) return r;
        }

        user.UpdatedAt = DateTime.UtcNow;
        return await userManager.UpdateAsync(user);
    }
}