using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

/// <summary>
/// Servicio encargado de gestionar los recargos.
/// </summary>
public class RecargoService(IRecargoRepository recargoRepository) : IRecargoService
{
    /// <inheritdoc cref="IRecargoService.CreateAsync(RecargoCreateRequestDto, CancellationToken)"/>
    public async Task<Result<RecargoResponseDto, DomainError>> CreateAsync(RecargoCreateRequestDto request, CancellationToken ct = default)
    {
        // Creamos el recargo
        var recargo = new Recargo
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            TipoPrecio = request.TipoPrecio,
            Importe = request.Importe,
            Activa = true
        };
        var creado = await recargoRepository.CreateAsync(recargo, ct);
        return Result.Success<RecargoResponseDto, DomainError>(MapToDto(creado));
    }

    /// <inheritdoc cref="IRecargoService.GetByIdAsync(long, CancellationToken)"/>
    public async Task<Result<RecargoResponseDto, DomainError>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        // Buscamos el recargo
        var recargo = await recargoRepository.GetByIdAsync(id, ct);

        if (recargo is null)
            return Result.Failure<RecargoResponseDto, DomainError>(
                NotFoundError.FromResource("Recargo", id.ToString()));
        return Result.Success<RecargoResponseDto, DomainError>(MapToDto(recargo));
    }

    /// <inheritdoc cref="IRecargoService.GetAllAsync(CancellationToken)"/>
    public async Task<Result<List<RecargoResponseDto>, DomainError>> GetAllAsync(CancellationToken ct = default)
    {
        // Obtenemos todos los recargos
        var recargos = await recargoRepository.GetAllAsync(ct);
        var resultado = recargos.Select(MapToDto).ToList();
        return Result.Success<List<RecargoResponseDto>, DomainError>(resultado);
    }

    /// <inheritdoc cref="IRecargoService.UpdateAsync(long, RecargoUpdateRequestDto, CancellationToken)"/>
    public async Task<Result<RecargoResponseDto, DomainError>> UpdateAsync(long id, RecargoUpdateRequestDto request, CancellationToken ct = default)
    {
        // Actualizamos el recargo
        var recargo = new Recargo
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            TipoPrecio = request.TipoPrecio,
            Importe = request.Importe
        };
        var actualizado = await recargoRepository.UpdateAsync(id, recargo, ct);
        if (actualizado is null)
            return Result.Failure<RecargoResponseDto, DomainError>(
                NotFoundError.FromResource("Recargo", id.ToString()));
        return Result.Success<RecargoResponseDto, DomainError>(MapToDto(actualizado));
    }

    /// <inheritdoc cref="IRecargoService.DeleteAsync(long, CancellationToken)"/>
    public async Task<Result<bool, DomainError>> DeleteAsync(long id, CancellationToken ct = default)
    {
        // Desactivamos el recargo
        var eliminado = await recargoRepository.DeleteAsync(id, ct);

        if (!eliminado)
            return Result.Failure<bool, DomainError>(
                NotFoundError.FromResource("Recargo", id.ToString()));
        return Result.Success<bool, DomainError>(true);
    }

    /// <summary>
    /// Convierte una entidad Recargo en su DTO de respuesta.
    /// </summary>
    private static RecargoResponseDto MapToDto(Recargo recargo)
    {
        return new RecargoResponseDto
        {
            Id = recargo.Id,
            Nombre = recargo.Nombre,
            Descripcion = recargo.Descripcion,
            TipoPrecio = recargo.TipoPrecio,
            Importe = recargo.Importe,
            Activa = recargo.Activa
        };
    }
}