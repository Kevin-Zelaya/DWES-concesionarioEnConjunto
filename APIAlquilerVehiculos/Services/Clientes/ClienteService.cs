using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Dtos.Common;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Errors;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Mappers;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Repositories.Clientes;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Services.Clientes;

public class ClienteService(IClienteRepository repository, IValidator<UpdateClienteDto> validatorUpdateAsync, ILogger<ClienteService> logger, UserManager<Usuario> userManager,
    AppDbContext dbContext,
    ICache cache) : IClienteService
{
    private static string CacheKey(long id) => $"cliente:{id}";
    
    /// <summary>
    /// Buscar TODOS los clientes con filtros y paginación
    /// </summary>
    /// <param name="dto">Filtros y paginación añadidos</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve una lista de clientes con o sin filtros, con paginación incluida</returns>
    public async Task<Result<PagedResult<ClienteDto>, DomainError>> FindAllPagedAsync(ClienteFilterDto dto, CancellationToken cancellationToken)
    {
        logger.LogInformation("Obteniendo clientes paginados - Página: {Page}, Tamaño: {Size}", dto.Page, dto.Size);

        // Obtenemos la tupla del repo (ClienteFilterDto)
        var (clientes, totalCount) = await repository.FindAllPagedAsync(dto, cancellationToken);
        
        // Mapeamos Cliente al ClienteDto
        var dtos = clientes.ToDtoList();

        var pagedResult = new PagedResult<ClienteDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = dto.Page + 1,
            PageSize = dto.Size
        };

        return Result.Success<PagedResult<ClienteDto>, DomainError>(pagedResult);
    }
    
    /// <summary>
    /// Busca un usuario específico
    /// </summary>
    /// <param name="id">Identificadoir del cliente</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuleve un cliente específico</returns>
    public async Task<Result<ClienteDto, DomainError>> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Buscando Funko {Id}", id);

        if (id <= 0)
        {
            logger.LogError("El ID {Id} no se ha encontrado", id);
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.NotFound(id));
        }
        
        var cached = await cache.GetTAsync<ClienteDto>(CacheKey(id));
        if (cached is not null) {
            logger.LogDebug("Cliente {Id} obtenido de la caché", id);
            return Result.Success<ClienteDto, DomainError>(cached);
        }
        
        var cliente = await repository.FindByIdAsync(id, cancellationToken); 
        
        if (cliente == null)
        {
            logger.LogWarning("Cliente {Id} NO encontrado ", id);
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.NotFound(id));
        } 
       
        var funkoResponse =  cliente.ToResponseDto();
        await cache.SetAsync(CacheKey(id), funkoResponse);
        return Result.Success<ClienteDto, DomainError>(funkoResponse);
    }
    
    /// <summary>
    /// Actualiza tanto los datos de cliente commo de su usuario
    /// </summary>
    /// <param name="id">Identificador del cliente a actualizar</param>
    /// <param name="updatedCliente">Datos del cliente actualizados</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve los datos del cliente ta actualizados</returns>
    public async Task<Result<ClienteDto, DomainError>> UpdateAsync(long id, UpdateClienteDto updatedCliente, CancellationToken cancellationToken)
    {
        var validationResult = await validatorUpdateAsync.ValidateAsync(updatedCliente, cancellationToken);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Hay errores de validación");
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.Validation(validationResult.ToDictionary()));
        }
        
        var clienteActual = await repository.FindByIdAsync(id, cancellationToken);
        if (id <= 0 || clienteActual is null)
        {
            logger.LogWarning("No se encontro el Funko {id}", id); 
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.NotFound(id));
        }
        
        // Comprobaciones de unicidad en BD (Email, DNI, NIE o Pasaporte ya existentes)
        var dniNorm = updatedCliente.Dni?.Trim().ToUpperInvariant();
        var nieNorm = updatedCliente.Nie?.Trim().ToUpperInvariant();
        var pasaporteNorm = updatedCliente.Pasaporte?.Trim().ToUpperInvariant();
        
        if (await dbContext.Clientes.AnyAsync(c =>
                (dniNorm != null && c.Dni == dniNorm) ||
                (nieNorm != null && c.Nie == nieNorm) ||
                (pasaporteNorm != null && c.Pasaporte == pasaporteNorm)))
        {
            var docDuplicado = dniNorm ?? nieNorm ?? pasaporteNorm ?? string.Empty;
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.DuplicateDocument(docDuplicado));
        }
        
        // Comprobamos que no existe otro Email identico
        if (await userManager.FindByEmailAsync(updatedCliente.Email) is not null)
        {
            logger.LogInformation("Ya existe un usuario con el email añadido: {Email}", updatedCliente.Email);
            return Result.Failure<ClienteDto, DomainError>(
                ClienteErrors.DuplicateEmail(updatedCliente.Email)
            );
        }
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            //Convertimos el RequestDTO al Modelo y validamos, si es null salta error
            var funkoModel = ClienteMapper.UpdateEntity(updatedCliente, id);

            //Actualizamos Cliente y comprobamos que no sea null, si lo es salta error
            var cliente = await repository.UpdateAsync(id, funkoModel, cancellationToken);
            if (cliente == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<ClienteDto, DomainError>(ClienteErrors.NotFound(id));
            }

            // email, teléfono y rol viven en Usuario / Identity
            var identityResult = await SincronizarUsuarioAsync(clienteActual, updatedCliente);
            if (!identityResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<ClienteDto, DomainError>(
                    ClienteErrors.IdentityError(identityResult.Errors.Select(e => e.Description)));
            }

            await transaction.CommitAsync(cancellationToken);
            await cache.RemoveAsync(CacheKey(id));

            //Transformamos a ResponseDTO
            var clienteResponse = ClienteMapper.ToResponseDto(cliente);
            logger.LogInformation("Cliente {id} actualizdo correctamente", id);

            //Devolvemos
            return Result.Success<ClienteDto, DomainError>(clienteResponse);
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.DuplicateDniOrEmail());
        }

        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<ClienteDto, DomainError>(
                ClienteErrors.ErrorServer("Error inesperado al modificar el cliente"));
        }

    }

    /// <summary>
    /// Da de baja a un cliente en específico
    /// </summary>
    /// <param name="id">Identificador del cliente a dar de baja</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>No devuleve nada si ha salido todo correcto</returns>
    public async Task<UnitResult<DomainError>> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        //Comprobamos que el Id existe, si no salta error
        var cliente = await repository.FindByIdAsync(id, cancellationToken);
        if (id <= 0 || cliente is null)
        {
            logger.LogWarning("Cliente {id} no encontrado", id);
            return UnitResult.Failure<DomainError>(ClienteErrors.NotFound(id));
        }
        
        // se comprueba que está activo
        if (!cliente.IsActivo) return UnitResult.Failure<DomainError>(ClienteErrors.YaInactivo(id));

        // comprueba si tiene reservas vivas
        if (await repository.TieneReservasVivasAsync(cliente.Id))
            return UnitResult.Failure<DomainError>(ClienteErrors.TieneReservasVivas(id));
        
        var deleted = await repository.DeleteAsync(id, cancellationToken);
        
        if (!deleted) return Result.Failure<bool, DomainError>(ClienteErrors.NotFound(id));
        
        // borra de caché
        await cache.RemoveAsync(CacheKey(id));
        
        logger.LogWarning("Cliente {id} dado de baja correctamente", id);
        
        return Result.Success<DomainError>();
    }
    
    /// <summary>
    /// Actualiza email y teléfono del Usuario Cliente. Solo toca lo que ha cambiado.
    /// </summary>
    private async Task<IdentityResult> SincronizarUsuarioAsync(Cliente actual, UpdateClienteDto dto)
    {
        var user = await userManager.FindByIdAsync(actual.IdUsuario.ToString());
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "No existe el usuario asociado al cliente" });

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

        user.UpdatedAt = DateTime.UtcNow;
        return await userManager.UpdateAsync(user);
    }
    
}