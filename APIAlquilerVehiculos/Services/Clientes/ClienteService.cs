using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Dtos;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Errors;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Mappers;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Repositories.Clientes;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    /// <param name="page">Número de pagina</param>
    /// <param name="pageSize">Número de elementos por pagina</param>
    /// <param name="incluirInactivos">Incluir clientes inactivos</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve una lista de clientes con o sin filtros, con paginación incluida</returns>
    public async Task<Result<IReadOnlyList<ClienteDto>, DomainError>> FindAllPagedAsync(CancellationToken cancellationToken, int page = 1, int pageSize = 10, bool incluirInactivos = false)
    {
        logger.LogInformation("Obteniendo clientes paginados - Página: {Page}, Tamaño: {Size}", page, pageSize);

        // evita skip negativos o páginas gigantes
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        
        var (items, _) = await repository.FindAllPagedAsync(page, pageSize, incluirInactivos, cancellationToken);
        var dtos = items.Select(t => t.ToResponseDto()).ToList();

        return Result.Success<IReadOnlyList<ClienteDto>, DomainError>(dtos);
    }
    
    /// <summary>
    /// Busca un usuario específico
    /// </summary>
    /// <param name="principal">Contexto de la seguridad para buscar el usuario que inició sesión</param>
    /// <param name="id">Identificadoir del cliente</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuleve un cliente específico</returns>
    public async Task<Result<ClienteDto, DomainError>> GetByIdAsync(ClaimsPrincipal principal, long id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Buscando cliente {Id}", id);

        if (id <= 0)
        {
            logger.LogError("El ID {Id} no se ha encontrado", id);
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.NotFound(id));
        }

        //Compobamos que el usuario tiene los permisos adecuados si busca mas de un clinte
        var error = await ClienteSinAutorizacionAsync(principal, id, cancellationToken, "buscar otros clientes.");
        if (error.IsFailure)
            return Result.Failure<ClienteDto, DomainError>(error.Error);
        
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
       
        var response =  cliente.ToResponseDto();
        await cache.SetAsync(CacheKey(id), response);
        return Result.Success<ClienteDto, DomainError>(response);
    }
    
    /// <summary>
    /// Actualiza tanto los datos de cliente commo de su usuario
    /// </summary>
    /// <param name="principal">Contexto de la seguridad para buscar el usuario que inició sesión</param>
    /// <param name="id">Identificador del cliente a actualizar</param>
    /// <param name="updatedCliente">Datos del cliente actualizados</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve los datos del cliente ta actualizados</returns>
    public async Task<Result<ClienteDto, DomainError>> UpdateAsync(ClaimsPrincipal principal, long id, UpdateClienteDto updatedCliente, CancellationToken cancellationToken)
    {
        var clienteActual = await repository.FindByIdAsync(id, cancellationToken);
        if (id <= 0 || clienteActual is null)
        {
            logger.LogWarning("No se encontro el Cliente {id}", id); 
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.NotFound(id));
        }
        
        var error = await ClienteSinAutorizacionAsync(principal, id, cancellationToken, "actualizar otros clientes.");
        if (error.IsFailure)
            return Result.Failure<ClienteDto, DomainError>(error.Error);
        
        var validationResult = await validatorUpdateAsync.ValidateAsync(updatedCliente, cancellationToken);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Hay errores de validación");
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.Validation(validationResult.ToDictionary()));
        }
        
        
        // Comprobaciones de unicidad en BD (Email, DNI, NIE o Pasaporte ya existentes)
        var dniNorm = updatedCliente.Dni?.Trim().ToUpperInvariant();
        var nieNorm = updatedCliente.Nie?.Trim().ToUpperInvariant();
        var pasaporteNorm = updatedCliente.Pasaporte?.Trim().ToUpperInvariant();
        
        if (await dbContext.Clientes.AnyAsync(c =>
                c.Id != id && (
                    (dniNorm != null && c.Dni == dniNorm) ||
                    (nieNorm != null && c.Nie == nieNorm) ||
                    (pasaporteNorm != null && c.Pasaporte == pasaporteNorm)
                ), cancellationToken))
        {
            var docDuplicado = dniNorm ?? nieNorm ?? pasaporteNorm ?? string.Empty;
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.DuplicateDocument(docDuplicado));
        }
        
        // Comprobamos que no existe otro Email identico
        var usuarioConMismoEmail = await userManager.FindByEmailAsync(updatedCliente.Email);
        if (usuarioConMismoEmail is not null && usuarioConMismoEmail.Id != clienteActual.IdUsuario)
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
            var clienteModel = ClienteMapper.UpdateEntity(updatedCliente, id);

            //Actualizamos Cliente y comprobamos que no sea null, si lo es salta error
            var cliente = await repository.UpdateAsync(id, clienteModel, cancellationToken);
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
            logger.LogError(ex, "Error al cargar cliente por duplicados");
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.DuplicateDniOrEmail());
        }

        catch (Exception ex)
        {
            logger.LogError(ex, "Error al cargar cliente");
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<ClienteDto, DomainError>(
                ClienteErrors.ErrorServer("Error inesperado al modificar el cliente"));
        }
    }
    
    /// <summary>
    /// Actualiza tanto los datos de cliente y por tanto de su usuario
    /// Solo admin
    /// </summary>
    /// <param name="id">Identificador del cliente a actualizar</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve los datos del cliente ta actualizados</returns>
    public async Task<Result<ClienteDto, DomainError>> UpdatePartialAsync(long id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            logger.LogWarning("No se encontro el Cliente {id}", id); 
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.NotFound(id));
        }
        var clienteActual = await repository.FindByIdAsync(id, cancellationToken);
        
        if (clienteActual is null)
        {
            logger.LogWarning("No se encontro el Cliente {id}", id); 
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.NotFound(id));
        }
        
        // se comprueba que está activo
        if (clienteActual.IsActivo)
        {
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.YaActivo(id));
        } 

        //Actualizamos Cliente y comprobamos que no sea null, si lo es salta error
        var cliente = await repository.UpdatePartialAsync(id, cancellationToken);
      
        if (cliente is null)
        {
            logger.LogWarning("No se actualizo el cliente {id}", id); 
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.NotFound(id));
        }

        //Transformamos a ResponseDTO
        var clienteResponse = ClienteMapper.ToResponseDto(cliente);
        //borrar de cache
        await cache.RemoveAsync(CacheKey(id));
        logger.LogInformation("Cliente {id} con estado renovado correctamente", id);
        
        //Devolvemos
        return Result.Success<ClienteDto, DomainError>(clienteResponse);
    }
    
    
    /// <summary>
    /// Da de baja a un cliente en específico
    /// </summary>
    /// <param name="principal">Contexto de la seguridad para buscar el usuario que inició sesión</param>
    /// <param name="id">Identificador del cliente a dar de baja</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>No devuleve nada si ha salido todo correcto</returns>
    public async Task<UnitResult<DomainError>> DeleteAsync(ClaimsPrincipal principal, long id, CancellationToken cancellationToken)
    {
        //Comprobamos que el Id existe, si no salta error
        var cliente = await repository.FindByIdAsync(id, cancellationToken);
        if (id <= 0 || cliente is null)
        {
            logger.LogWarning("Cliente {id} no encontrado", id);
            return UnitResult.Failure<DomainError>(ClienteErrors.NotFound(id));
        }
        
        var error = await ClienteSinAutorizacionAsync(principal, id, cancellationToken, "dar de baja otros clientes.");
        if (error.IsFailure)
            return UnitResult.Failure<DomainError>(error.Error);
        
        // se comprueba que está activo
        if (!cliente.IsActivo) return UnitResult.Failure<DomainError>(ClienteErrors.YaInactivo(id));

        // comprueba si tiene reservas vivas
        if (await repository.TieneReservasVivasAsync(cliente.Id))
            return UnitResult.Failure<DomainError>(ClienteErrors.TieneReservasVivas(id));
        
        var deleted = await repository.DeleteAsync(id, cancellationToken);
        
        if (!deleted) return UnitResult.Failure<DomainError>(ClienteErrors.NotFound(id));
        
        // borra de caché
        await cache.RemoveAsync(CacheKey(id));
        
        logger.LogWarning("Cliente {id} dado de baja correctamente", id);
        
        return UnitResult.Success<DomainError>();
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

    private async Task<UnitResult<DomainError>> ClienteSinAutorizacionAsync(ClaimsPrincipal principal, long id, CancellationToken cancellationToken, string message)
    {
        //COMPROBAMOS QUE USUARIO ES 
        var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier) 
                        ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!long.TryParse(userIdStr, out var userId))
        {
            return UnitResult.Failure<DomainError>(ClienteErrors.UserNotFound());
        }
        var user = await userManager.Users
            .Include(u => u.Cliente)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
       
        if (user is null)
        {
            logger.LogError("Usuario autenticado con ID {UserId} no encontrado en BD", userId);
            return UnitResult.Failure<DomainError>(ClienteErrors.UserNotFound());
        }
        
        var role = await userManager.GetRolesAsync(user);
        
        if (role.Contains(Roles.Cliente) && (user.Cliente is null || id != user.Cliente.Id))
        {
            return UnitResult.Failure<DomainError>(ClienteErrors.PermisosInsuficientes(message));
        }
        return UnitResult.Success<DomainError>();
    }
}