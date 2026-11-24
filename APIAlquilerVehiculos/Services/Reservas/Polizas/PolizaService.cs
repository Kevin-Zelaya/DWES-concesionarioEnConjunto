using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

/// <summary>
/// Servicio encargado de gestionar las pólizas.
/// </summary>
public class PolizaService(IPolizaRepository polizaRepository) : IPolizaService
{
    /// <inheritdoc cref="IPolizaService.CreateAsync(PolizaCreateRequestDto, CancellationToken)"/>
    public async Task<Result<PolizaResponseDto, DomainError>> CreateAsync(PolizaCreateRequestDto request, CancellationToken ct = default)
    {
        // Creamos la póliza
        var poliza = new Poliza
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            IncrementoDiario = request.IncrementoDiario,
            Activa = true
        };

        var creada = await polizaRepository.CreateAsync(poliza, ct);

        return Result.Success<PolizaResponseDto, DomainError>(MapToDto(creada));
    }

    /// <inheritdoc cref="IPolizaService.GetByIdAsync(long, CancellationToken)"/>
    public async Task<Result<PolizaResponseDto, DomainError>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        // Buscamos la póliza
        var poliza = await polizaRepository.GetByIdAsync(id, ct);

        if (poliza is null)
            return Result.Failure<PolizaResponseDto, DomainError>(
                NotFoundError.FromResource("Póliza", id.ToString()));

        return Result.Success<PolizaResponseDto, DomainError>(MapToDto(poliza));
    }

    /// <inheritdoc cref="IPolizaService.GetAllAsync(CancellationToken)"/>
    public async Task<Result<List<PolizaResponseDto>, DomainError>> GetAllAsync(CancellationToken ct = default)
    {
        // Obtenemos todas las pólizas
        var polizas = await polizaRepository.GetAllAsync(ct);

        var resultado = polizas.Select(MapToDto).ToList();

        return Result.Success<List<PolizaResponseDto>, DomainError>(resultado);
    }

    /// <inheritdoc cref="IPolizaService.UpdateAsync(long, PolizaUpdateRequestDto, CancellationToken)"/>
    public async Task<Result<PolizaResponseDto, DomainError>> UpdateAsync(long id, PolizaUpdateRequestDto request, CancellationToken ct = default)
    {
        // Actualizamos la póliza
        var poliza = new Poliza
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            IncrementoDiario = request.IncrementoDiario
        };

        var actualizada = await polizaRepository.UpdateAsync(id, poliza, ct);

        if (actualizada is null)
            return Result.Failure<PolizaResponseDto, DomainError>(
                NotFoundError.FromResource("Póliza", id.ToString()));

        return Result.Success<PolizaResponseDto, DomainError>(MapToDto(actualizada));
    }

    /// <inheritdoc cref="IPolizaService.DeleteAsync(long, CancellationToken)"/>
    public async Task<Result<bool, DomainError>> DeleteAsync(long id, CancellationToken ct = default)
    {
        // Desactivamos la póliza
        var eliminada = await polizaRepository.DeleteAsync(id, ct);

        if (!eliminada)
            return Result.Failure<bool, DomainError>(
                NotFoundError.FromResource("Póliza", id.ToString()));

        return Result.Success<bool, DomainError>(true);
    }

    /// <summary>
    /// Convierte una entidad Poliza en su DTO de respuesta.
    /// </summary>
    private static PolizaResponseDto MapToDto(Poliza poliza)
    {
        return new PolizaResponseDto
        {
            Id = poliza.Id,
            Nombre = poliza.Nombre,
            Descripcion = poliza.Descripcion,
            IncrementoDiario = poliza.IncrementoDiario,
            Activa = poliza.Activa
        };
    }
}