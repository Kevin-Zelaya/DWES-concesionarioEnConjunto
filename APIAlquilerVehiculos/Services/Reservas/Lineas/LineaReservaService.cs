using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Errors.Reservas;
using CSharpFunctionalExtensions;

/// <summary>
/// Implementa la lógica de negocio para la gestión de las líneas de reserva.
/// </summary>
/// <param name="_repository">Repositorio para acceder a las líneas de reserva.</param>
/// <param name="_logger">Servicio de registro de eventos e incidencias.</param>
public class LineaReservaService(
    ILineaReservaRepository _repository,
    ILogger<LineaReservaService> _logger
) : ILineaReservaService
{
    // =========================================================
    // CREATE
    // =========================================================

    /// <inheritdoc cref="ILineaReservaService.CreateLineasReservaAsync(List{LineaReserva}, int, CancellationToken)"/>
    public async Task<Result<List<LineaReserva>, DomainError>> CreateLineasReservaAsync(List<LineaReserva> lineas, int dias, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-CREATE] Creando {Cantidad} líneas para la reserva {ReservaId}.",
            lineas.Count, lineas.FirstOrDefault()?.ReservaId);

        // Procesar las líneas nuevas y calcular sus subtotales.
        var nuevas = await ProcesarNuevas(lineas, dias, ct);

        nuevas.ForEach(l => l.CalcularSubtotal());

        var response = await _repository.CreateRangeAsync(nuevas, ct);

        return Result.Success<List<LineaReserva>, DomainError>(response)
            .Tap(l => _logger.LogInformation(
                "[SERVICE-CREATE] Se han creado {Cantidad} líneas correctamente para la reserva {ReservaId}.",
                l.Count, l.FirstOrDefault()?.ReservaId));
    }

    /// <summary>
    /// Procesa las nuevas líneas de una reserva, agrupando aquellas que tienen las mismas características.
    /// Si ya existe una línea equivalente, incrementa su cantidad y recalcula su subtotal.
    /// </summary>
    /// <param name="lineasNuevas">Líneas que se desean incorporar a la reserva.</param>
    /// <param name="dias">Número de días de alquiler para calcular los importes.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Lista de líneas que deben añadirse a la reserva.</returns>
    private async Task<List<LineaReserva>> ProcesarNuevas(List<LineaReserva> lineasNuevas, int dias, CancellationToken ct)
    {
        var existentes = await _repository.GetByReservaIdAsync(lineasNuevas.First().ReservaId, ct);

        var nuevas = new List<LineaReserva>();

        foreach (var linea in lineasNuevas)
        {
            // Comprobar si existe alguna con las mismas características para sumar.
            var existente = existentes.FirstOrDefault(l =>
                l.Tipo == linea.Tipo &&
                l.Concepto == linea.Concepto &&
                l.PrecioUnitario == linea.PrecioUnitario &&
                l.TipoPrecio == linea.TipoPrecio);

            if (existente is not null)
            {
                existente.Cantidad += linea.Cantidad;
                existente.CalcularSubtotal();
            }
            else
            {
                nuevas.Add(linea);
            }
        }

        return nuevas;
    }

    /// <inheritdoc cref="ILineaReservaService.CalculateTotalByLineas(List{LineaReserva})"/>
    public decimal CalculateTotalByLineas(List<LineaReserva> lineas)
    {
        _logger.LogDebug("[SERVICE-CALCULATE] Calculando total a partir de las líneas disponibles.");
        return lineas.Sum(l => l.Subtotal);
    }

    // =========================================================
    // READ
    // =========================================================

    /// <inheritdoc cref="ILineaReservaService.GetLineaReservaByIdAsync(long, CancellationToken)"/>
    public async Task<Result<LineaReserva, DomainError>> GetLineaReservaByIdAsync(long id, CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-GET] Buscando línea de reserva con ID {Id}.", id);

        var response = await _repository.GetByIdAsync(id, ct);

        if (response is null)
            return Result.Failure<LineaReserva, DomainError>(LineaErrors.NotFound(id))
                .Tap(_ => _logger.LogWarning("[SERVICE-GET] No se encontró la línea de reserva {Id}.", id));

        return Result.Success<LineaReserva, DomainError>(response)
            .Tap(l => _logger.LogInformation("[SERVICE-GET] Línea {Id} recuperada correctamente.", l.Id));
    }

    /// <inheritdoc cref="ILineaReservaService.GetLineasByReservaIdAsync(long, CancellationToken)"/>
    public async Task<Result<List<LineaReserva>, DomainError>> GetLineasByReservaIdAsync(long reservaId, CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-GET] Recuperando líneas de la reserva {ReservaId}.", reservaId);

        var response = await _repository.GetByReservaIdAsync(reservaId, ct);

        return Result.Success<List<LineaReserva>, DomainError>(response)
            .Tap(l => _logger.LogInformation(
                "[SERVICE-GET] Se recuperaron {Count} líneas de la reserva {ReservaId}.",
                l.Count, reservaId));
    }

    // =========================================================
    // UPDATE
    // =========================================================

    /// <inheritdoc cref="ILineaReservaService.UpdateLineaReservaAsync(long, LineaReserva, CancellationToken)"/>
    public async Task<Result<LineaReserva, DomainError>> UpdateLineaReservaAsync(long id, LineaReserva linea, CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-UPDATE] Actualizando línea de reserva {Id}.", id);

        var response = await _repository.UpdateAsync(id, linea, ct);

        if (response is null)
            return Result.Failure<LineaReserva, DomainError>(LineaErrors.NotFound(id))
                .Tap(_ => _logger.LogWarning("[SERVICE-UPDATE] No se encontró la línea de reserva {Id}.", id));

        return Result.Success<LineaReserva, DomainError>(response)
            .Tap(l => _logger.LogInformation("[SERVICE-UPDATE] Línea {Id} actualizada correctamente.", l.Id));
    }

    // =========================================================
    // DELETE
    // =========================================================

    /// <inheritdoc cref="ILineaReservaService.DeleteLineaReservaAsync(long, CancellationToken)"/>
    public async Task<Result<bool, DomainError>> DeleteLineaReservaAsync(long id, CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-DELETE] Eliminando línea de reserva {Id}.", id);

        var response = await _repository.DeleteAsync(id, ct);

        if (!response)
            return Result.Failure<bool, DomainError>(LineaErrors.NotFound(id))
                .Tap(_ => _logger.LogWarning("[SERVICE-DELETE] No se encontró la línea de reserva {Id}.", id));

        return Result.Success<bool, DomainError>(true)
            .Tap(_ => _logger.LogInformation("[SERVICE-DELETE] Línea {Id} eliminada correctamente.", id));
    }
}