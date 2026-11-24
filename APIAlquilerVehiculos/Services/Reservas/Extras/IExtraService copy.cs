using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Models;
using CSharpFunctionalExtensions;

/// <summary>
/// Servicio encargado de gestionar los extras.
/// </summary>
public class ExtraService(IExtraRepository extraRepository) : IExtraService
{
    /// <inheritdoc cref="IExtraService.CreateAsync(ExtraCreateRequestDto, CancellationToken)"/>
    public async Task<Result<ExtraResponseDto, DomainError>> CreateAsync(ExtraCreateRequestDto request, CancellationToken ct = default)
    {
        // Creamos el extra
        var extra = new Extra
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            TipoPrecio = request.TipoPrecio,
            Activa = true
        };

        var creado = await extraRepository.CreateAsync(extra, ct);

        return Result.Success<ExtraResponseDto, DomainError>(MapToDto(creado));
    }

    /// <inheritdoc cref="IExtraService.GetByIdAsync(long, CancellationToken)"/>
    public async Task<Result<ExtraResponseDto, DomainError>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        // Buscamos el extra
        var extra = await extraRepository.GetByIdAsync(id, ct);

        if (extra is null)
            return Result.Failure<ExtraResponseDto, DomainError>(
                NotFoundError.FromResource("Extra", id.ToString()));

        return Result.Success<ExtraResponseDto, DomainError>(MapToDto(extra));
    }

    /// <inheritdoc cref="IExtraService.GetAllAsync(CancellationToken)"/>
    public async Task<Result<List<ExtraResponseDto>, DomainError>> GetAllAsync(CancellationToken ct = default)
    {
        // Obtenemos todos los extras
        var extras = await extraRepository.GetAllAsync(ct);

        var resultado = extras.Select(MapToDto).ToList();

        return Result.Success<List<ExtraResponseDto>, DomainError>(resultado);
    }

    /// <inheritdoc cref="IExtraService.UpdateAsync(long, ExtraUpdateRequestDto, CancellationToken)"/>
    public async Task<Result<ExtraResponseDto, DomainError>> UpdateAsync(long id, ExtraUpdateRequestDto request, CancellationToken ct = default)
    {
        // Actualizamos el extra
        var extra = new Extra
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            TipoPrecio = request.TipoPrecio,
            Activa = request.Activa
        };

        var actualizado = await extraRepository.UpdateAsync(id, extra, ct);

        if (actualizado is null)
            return Result.Failure<ExtraResponseDto, DomainError>(
                NotFoundError.FromResource("Extra", id.ToString()));

        return Result.Success<ExtraResponseDto, DomainError>(MapToDto(actualizado));
    }

    /// <inheritdoc cref="IExtraService.DeleteAsync(long, CancellationToken)"/>
    public async Task<Result<bool, DomainError>> DeleteAsync(long id, CancellationToken ct = default)
    {
        // Desactivamos el extra
        var eliminado = await extraRepository.DeleteAsync(id, ct);

        if (!eliminado)
            return Result.Failure<bool, DomainError>(
                NotFoundError.FromResource("Extra", id.ToString()));

        return Result.Success<bool, DomainError>(true);
    }

    /// <summary>
    /// Convierte una entidad Extra en su DTO de respuesta.
    /// </summary>
    private static ExtraResponseDto MapToDto(Extra extra)
    {
        return new ExtraResponseDto
        {
            Id = extra.Id,
            Nombre = extra.Nombre,
            Descripcion = extra.Descripcion,
            Precio = extra.Precio,
            TipoPrecio = extra.TipoPrecio,
            Activa = extra.Activa
        };
    }
}