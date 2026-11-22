using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Dtos.Common;
using APIAlquilerVehiculos.Errors;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Mappers;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Repositories.Clientes;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Services.Clientes;

public class ClienteService(IClienteRepository repository, IValidator<UpdateClienteDto> _validatorUpdateAsync, ILogger<ClienteService> logger) : IClienteService
{
    public async Task<Result<PagedResult<ClienteDto>, DomainError>> FindAllPagedAsync(ClienteFilterDto dto)
    {
        logger.LogInformation("Obteniendo clientes paginados - Página: {Page}, Tamaño: {Size}", dto.Page, dto.Size);

        // Obtenemos la tupla del repo (ClienteFilterDto)
        var (clientes, totalCount) = await repository.FindAllPagedAsync(dto);
        
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
    
    public async Task<Result<ClienteDto, DomainError>> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Buscando Funko {Id}", id);
        if (id <= 0) return Result.Failure<ClienteDto, DomainError>(ClienteErrors.NotFound(id));
        Cliente? cliente = await repository.FindByIdAsync(id, cancellationToken); 
        logger.LogWarning("Funko {Id} NO encontrado ", id);
        if (cliente == null)
        {
            logger.LogWarning("Funko {Id} NO encontrado ", id); 
            return Result.Failure<ClienteDto, DomainError>(ClienteErrors.NotFound());
        } 
       
        ClienteDto funkoResponse =  ClienteMapper.ToResponseDto(cliente);
        return Result.Success<ClienteDto, DomainError>(funkoResponse);
    }
    
    public async Task<Result<ClienteDto, DomainError>> UpdateAsync(long id, FunkoUpdateDto updatedFunko, CancellationToken cancellationToken)
    {
        // Si la validación falla, ModelState.IsValid será false automáticamente
        var context = new ValidationContext<FunkoUpdateDto>(updatedFunko);
        context.RootContextData["FunkoId"] = id;
        var validationResult = await _validatorUpdateAsync.ValidateAsync(context);
        if (!validationResult.IsValid)
        {
            logger.LogInformation("Hay errores de validación");
            // Si falla, mapeamos los errores de FluentValidation a tu sistema de DomainErrors
            var errorsDescription = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<FunkosDtoResponse, DomainErrors>(ClienteErrors.BadRequest(errorsDescription));
        }
        
        //Buscamos el funko por Id y comprobamos si existe, si no salta error
        var funkoId = await repository.FindByIdAsync(id, cancellationToken);
        if (id <= 0 || funkoId is null) { logger.LogWarning("No se encontro el Funko {id}", id); return Result.Failure<FunkosDtoResponse, DomainErrors>(ClienteErrors.NotFound(id));}
        
        //Convertimos el RequestDTO al Modelo y validamos, si es null salta error
        var funkoModel = ClienteMapper.ToEntityUpdate(updatedFunko);
        if (funkoModel is null) {logger.LogWarning("No guardo el Funko {id}", id); return Result.Failure<FunkosDtoResponse, DomainErrors>(ClienteErrors.NotFound());} 
        //Comprobamos que el Categoria actualizado NO tiene un nombre igual a otro Categoria
     
        // --- GESTIÓN DE LA IMAGEN EN ACTUALIZACIÓN ---
        if (updatedFunko.Image is not null && updatedFunko.Image.Length > 0)
        {
            var storageResult = await storage.StoreAsync(updatedFunko.Image, "funkos", cancellationToken);
            
            if (storageResult.IsFailure)
            {
                logger.LogWarning("Error al guardar la imagen del Funko");
                return Result.Failure<ClienteDto, DomainError>(storageResult.Error);
            }
            // Guardamos la nueva imagen
            var fileName = storageResult.Value;
            funkoModel.Imagen = storage.GetUrl(fileName, "funkos");

            // Borramos la imagen vieja del disco si no es la por defecto
            if (!string.IsNullOrEmpty(funkoId.Imagen) && !funkoId.Imagen.EndsWith("default.png"))
            {
                var oldFileName = IOPath.GetFileName(funkoId.Imagen);
                await storage.DeleteAsync(oldFileName, "funkos", cancellationToken);
            }
        }
        else
        {
            // Si no se adjunta nueva imagen en el Update, mantenemos la que ya tenía guardada
            funkoModel.Imagen = funkoId.Imagen;
            if(funkoId.Imagen is null){funkoModel.Imagen = "/uploads/funkos/default.png";}
        }
        
       
       try
       {
           //Actualizamos Funko y comprobamos que no sea null, si lo es salta error
           Funko? funko = await repository.UpdateAsync(id, funkoModel, cancellationToken);
           if (funko == null) return Result.Failure<ClienteDto, DomainError>(ClienteErrors.NotFound());

           //Transformamos a ResponseDTO
           ClienteDto funkoResponse = FunkoMapper.ToDtoResponse(funko);
           logger.LogInformation("Funko {id} actualizdo correctamente", id);
           
           // Notificar a admins
           await hubContext.Clients.Group("admins").SendAsync("FunkoActualizado", new 
           { 
               Id = id,
               Mensaje = $"El Funko con ID {id} ha sido actualizado." 
           });
           
           //Devolvemos
           return Result.Success<FunkosDtoResponse, DomainErrors>(funkoResponse);
       }
       catch (DbUpdateConcurrencyException ex)
       {
           logger.LogWarning(ex, "Conflicto de concurrencia al actualizar el Funko {id}", id);
           var entry = ex.Entries.First();
           var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

           if (databaseValues is null)
           {
               return Result.Failure<ClienteDto, DomainError>(CategoriaError.NotFound(id));
           }
           
           return Result.Failure<ClienteDto, DomainError>(ClienteErrors.Concurrency("El Funko ",id, "modificado "));
       }
    }

    public async Task<Result<bool, DomainError>> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        //Comprobamos que el Id existe, si no salta error
        var funkoId = await repository.FindByIdAsync(id, cancellationToken);
        if (id <= 0 || funkoId is null) return Result.Failure<bool, DomainError>(ClienteErrors.NotFound(id));
        
        try
        {
            var deleted = await repository.DeleteAsync(id, cancellationToken);
        
            if (!deleted) return Result.Failure<bool, DomainError>(ClienteErrors.NotFound(funkoId.Name));
            
            logger.LogWarning("Funko {id} eliminado correctamente", id);
            
            // Notificar a admins
            await hubContext.Clients.Group("admins").SendAsync("FunkoEliminado", new 
            { 
                Id = id,
                Mensaje = $"El Funko con ID {id} ha sido eliminado." 
            });

            
            return Result.Success<bool, DomainError>(true);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Conflicto de concurrencia al actualizar el Funko {id}", id);
            var entry = ex.Entries.First();
            var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

            if (databaseValues is null)
            {
                return Result.Failure<bool, DomainError>(CategoriaError.NotFound(id));
            }
           
            return Result.Failure<bool, DomainError>(ClienteErrors.Concurrency("El Funko ",id, "eliminado "));
        }
    }
    
}