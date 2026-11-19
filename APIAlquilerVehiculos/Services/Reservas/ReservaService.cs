using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Errors.Reservas;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Repositories.Vehiculos;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

public class ReservaService(
    IReservaRepository _repository,
    ILogger<ReservaService> _logger,
    IUnitOfWork _unitOfWork,
    IVehiculosRepository _vehiculoRepository,
    ILineaReservaService _lineaService,
    ILineaReservaFactory _lineaReservaFactory
) : IReservaService
{
    // =========================================================
    // CREATE
    // =========================================================
    /// <inheritdoc cref="IReservaService.CreateReservaAsync(ReservaCreateRequestDto, long, CancellationToken)"/>
    public async Task<Result<ReservaDto, DomainError>> CreateReservaAsync(ReservaCreateRequestDto reserva, long TrabajadorId, CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-CREATE] Intentando crear una reserva.");
        //1. comprobamos si existe el vehiculo y el cliente
        var vehiculo = await _vehiculoRepository.GetByIdAsync(reserva.VehiculoId);
        if(vehiculo is null)
            return Result.Failure<ReservaDto, DomainError>(
                NotFoundError.FromResource("Vehículo", reserva.VehiculoId)
            );
        // comprobar en el futuro si existe un cliente
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var lineasFactorizadas = await FactorizarLineas(reserva.Lineas, vehiculo, ct);
            if (lineasFactorizadas.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return Result.Failure<ReservaDto, DomainError>(lineasFactorizadas.Error);
            }
            var reservaTS = reserva.ToEntity(TrabajadorId);
            // Se guarda sin asignar aún el total
            reservaTS = await _repository.CreateAsync(reservaTS, ct);
            // Crear lineas
            lineasFactorizadas.Value.ForEach(l => l.ReservaId = reservaTS.Id);
            var lineasCreadas = await _lineaService.CreateLineasReservaAsync(
                lineasFactorizadas.Value, 
                reservaTS.CalcularDiasAlquiler(),
                ct);

            if (lineasCreadas.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return Result.Failure<ReservaDto, DomainError>(lineasCreadas.Error);
            }
            var total = _lineaService.CalculateTotalByLineas(lineasCreadas.Value);
            reservaTS.PrecioTotal = total;
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            reservaTS.Lineas = lineasCreadas.Value;
            
            return Result.Success<ReservaDto, DomainError>(reservaTS.ToDto())
                .Tap(r => _logger.LogInformation(
                    "[SERVICE-CREATE] Reserva creada correctamente con ID {Id} y código {Codigo}.",
                    r.Id, r.CodigoRefencia));
        } catch(Exception)
        {
            _logger.LogError("[SERVICE-CREATE] Error al crear la reserva, todos los cambios son eliminados.");
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
    private async Task<Result<List<LineaReserva>, DomainError>> FactorizarLineas(
        List<LineaReservaCreateRequestDto> lineas,
        Vehiculo vehiculo,
        CancellationToken ct)
    {
        var lineasFd = new List<LineaReserva>
        {
            CrearLineaVehiculo(vehiculo)
        };

        foreach (var linea in lineas)
        {
            var resultado = await _lineaReservaFactory.CreateAsync(linea, ct);

            if (resultado.IsFailure)
                return Result.Failure<List<LineaReserva>, DomainError>(
                    resultado.Error);

            lineasFd.Add(resultado.Value);
        }

        return Result.Success<List<LineaReserva>, DomainError>(lineasFd);
    }
    private static LineaReserva CrearLineaVehiculo(Vehiculo vehiculo)
    {
        return new LineaReserva
        {
            Tipo = TipoLinea.Vehiculo,
            Concepto = "Vehiculo con id"+vehiculo.Id,
            Cantidad = 1,
            PrecioUnitario = vehiculo.precioPorDia,
            TipoPrecio = TipoPrecio.Diario
        };
    }

    // =========================================================
    // READ
    // =========================================================
    /// <inheritdoc cref="IReservaService.GetReservaByIdAsync(long, CancellationToken)(CancellationToken)(long, int, int, CancellationToken)"/>
    public async Task<Result<Reserva, DomainError>> GetReservaByIdAsync(long id, CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-GET] Buscando reserva con ID {Id}.", id);

        var response = await _repository.GetByIdAsync(id, ct);

        if (response is null)
            return Result.Failure<Reserva, DomainError>( 
                ReservaErrors.NotFound(id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-GET] No se encontró la reserva con ID {Id}.", id));

        return Result.Success<Reserva, DomainError>(response)
            .Tap(r => _logger.LogInformation(
                "[SERVICE-GET] Reserva {Codigo} recuperada correctamente.",
                r.CodigoRefencia));
    }

    /// <inheritdoc cref="IReservaService.GetReservaByCodigoReferenciaAsync(string, CancellationToken)"/>
    public async Task<Result<Reserva, DomainError>> GetReservaByCodigoReferenciaAsync(string codigo, CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-GET] Buscando reserva con código {Codigo}.", codigo);

        var response = await _repository.GetReservaByCodigoReferenciaAsync(codigo, ct);

        if (response is null)
            return Result.Failure<Reserva, DomainError>(
                ReservaErrors.NotFound(codigo))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-GET] No se encontró la reserva con código {Codigo}.", codigo));

        return Result.Success<Reserva, DomainError>(response)
            .Tap(r => _logger.LogInformation(
                "[SERVICE-GET] Reserva {Codigo} recuperada correctamente.",
                r.CodigoRefencia));
    }

    /// <inheritdoc cref="IReservaService.GetAllReservasAsync(CancellationToken)"/>
    public async Task<Result<List<Reserva>, DomainError>> GetAllReservasAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-GETALL] Recuperando todas las reservas.");

        var response = await _repository.GetAllAsync(ct);

        var reservas = response.ToList();

        return Result.Success<List<Reserva>, DomainError>(reservas)
            .Tap(_ => _logger.LogInformation(
                "[SERVICE-GETALL] Se recuperaron {Count} reservas.",
                reservas.Count));
    }

    // =========================================================
    // PAGINATION
    // =========================================================
    /// <inheritdoc cref="IReservaService.GetPaginatedReservasAsync(int, int, CancellationToken)"/>
    public async Task<Result<PaginatedResouce<Reserva>, DomainError>> GetPaginatedReservasAsync(int pageNumber, int pageSize, CancellationToken ct)
    {
        _logger.LogDebug(
            "[SERVICE-GETPAGINATED] Recuperando reservas. Página {PageNumber}, tamaño {PageSize}.",
            pageNumber, pageSize);

        var response = await _repository.GetPaginatedReservasAsync(pageNumber, pageSize, ct);
        var toResponse = new PaginatedResouce<Reserva>(
            response.items,
            response.count,
            response.totalPages,
            response.HasPreviousPage,
            response.HasNextPage
        );
        return Result.Success<PaginatedResouce<Reserva>, DomainError>(toResponse)
            .Tap(r => _logger.LogInformation(
                "[SERVICE-GETPAGINATED] Se recuperaron {Count} reservas de la página {PageNumber}.",
                r.Count, pageNumber));
    }

    /// <inheritdoc cref="IReservaService.GetPaginatedReservasByCustomerIdAsync(long, int, int, CancellationToken)"/>
    public async Task<Result<PaginatedResouce<Reserva>, DomainError>> GetPaginatedReservasByCustomerIdAsync(long id, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-GETPAGINATED] Recuperando reservas del cliente {Id}.",
            id);

        var response = await _repository.GetPaginatedReservasByCustomerIdAsync(id, pageNumber, pageSize, ct);
        var toResponse = new PaginatedResouce<Reserva>(
            response.items,
            response.count,
            response.totalPages,
            response.HasPreviousPage,
            response.HasNextPage
        );
        return Result.Success<PaginatedResouce<Reserva>, DomainError>(toResponse)
            .Tap(r => _logger.LogInformation(
                "[SERVICE-GETPAGINATED] Se recuperaron {Count} reservas del cliente {Id}.",
                r.Count, pageNumber));
    }

    /// <inheritdoc cref="IReservaService.GetPaginatedReservasByEmployeeIdAsync(long, int, int, CancellationToken)"/>
    public async Task<Result<PaginatedResouce<Reserva>, DomainError>> GetPaginatedReservasByEmployeeIdAsync(long id, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-GETPAGINATED] Recuperando reservas del trabajador {Id}.",
            id);

        var response = await _repository.GetPaginatedReservasByEmployeeIdAsync(id, pageNumber, pageSize, ct);
        var toResponse = new PaginatedResouce<Reserva>(
            response.items,
            response.count,
            response.totalPages,
            response.HasPreviousPage,
            response.HasNextPage
        );
        return Result.Success<PaginatedResouce<Reserva>, DomainError>(toResponse)
            .Tap(r => _logger.LogInformation(
                "[SERVICE-GETPAGINATED] Se recuperaron {Count} reservas del trabajador {Id}.",
                r.Count, pageNumber));
    }

    // =========================================================
    // UPDATE
    // =========================================================

    /// <inheritdoc cref="IReservaService.UpdateNotaAsync(string, ReservaUpdateNotaRequestDto, CancellationToken)"/>
   public async Task<Result<Reserva, DomainError>> UpdateNotaAsync(string codigoReferencia, ReservaUpdateNotaRequestDto request, CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-UPDATE] Actualizando notas de la reserva {Codigo}.", codigoReferencia);

        var response = await _repository.UpdateNotaAsync(codigoReferencia, request, ct);

        if (response is null)
            return Result.Failure<Reserva, DomainError>(ReservaErrors.NotFound(codigoReferencia))
                .Tap(_ => _logger.LogWarning("[SERVICE-UPDATE] No se encontró la reserva {Codigo}.", codigoReferencia));

        return Result.Success<Reserva, DomainError>(response)
            .Tap(r => _logger.LogInformation("[SERVICE-UPDATE] Notas de la reserva {Codigo} actualizadas correctamente.", r.CodigoRefencia));
    }

    /// <inheritdoc cref="IReservaService.UpdatePagoAsync(string, ReservaUpdatePagoRequestDto, CancellationToken)"/>
    public async Task<Result<Reserva, DomainError>> UpdatePagoAsync(string codigoReferencia, ReservaUpdatePagoRequestDto request, CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-UPDATE] Actualizando pago de la reserva {Codigo}.", codigoReferencia);

        var response = await _repository.UpdatePagoAsync(codigoReferencia, request, ct);

        if (response is null)
            return Result.Failure<Reserva, DomainError>(ReservaErrors.NotFound(codigoReferencia))
                .Tap(_ => _logger.LogWarning("[SERVICE-UPDATE] No se encontró la reserva {Codigo}.", codigoReferencia));

        return Result.Success<Reserva, DomainError>(response)
            .Tap(r => _logger.LogInformation("[SERVICE-UPDATE] Pago de la reserva {Codigo} actualizado correctamente.", r.CodigoRefencia));
    }

    /// <inheritdoc cref="IReservaService.UpdateFechasAsync(string, ReservaUpdateFechaRequestDto, CancellationToken)"/>
    public async Task<Result<Reserva, DomainError>> UpdateFechasAsync(string codigoReferencia, ReservaUpdateFechaRequestDto request, CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-UPDATE] Actualizando fechas de la reserva {Codigo}.", codigoReferencia);

        var response = await _repository.UpdateFechasAsync(codigoReferencia, request, ct);

        if (response is null)
            return Result.Failure<Reserva, DomainError>(ReservaErrors.NotFound(codigoReferencia))
                .Tap(_ => _logger.LogWarning("[SERVICE-UPDATE] No se encontró la reserva {Codigo}.", codigoReferencia));

        return Result.Success<Reserva, DomainError>(response)
            .Tap(r => _logger.LogInformation("[SERVICE-UPDATE] Fechas de la reserva {Codigo} actualizadas correctamente.", r.CodigoRefencia));
    }

    /// <inheritdoc cref="IReservaService.UpdateStatusAsync(string, ReservaUpdateStatusRequestDto, CancellationToken)"/>
    public async Task<Result<Reserva, DomainError>> UpdateStatusAsync(string codigoReferencia, ReservaUpdateStatusRequestDto request, CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-UPDATE] Actualizando estado de la reserva {Codigo}.", codigoReferencia);

        var response = await _repository.UpdateStatusAsync(codigoReferencia, request, ct);

        if (response is null)
            return Result.Failure<Reserva, DomainError>(ReservaErrors.NotFound(codigoReferencia))
                .Tap(_ => _logger.LogWarning("[SERVICE-UPDATE] No se encontró la reserva {Codigo}.", codigoReferencia));

        return Result.Success<Reserva, DomainError>(response)
            .Tap(r => _logger.LogInformation("[SERVICE-UPDATE] Estado de la reserva {Codigo} actualizado correctamente.", r.CodigoRefencia));
    }

    /// <inheritdoc cref="IReservaService.UpdateVehicleAsync(string, ReservaUpdateVehicleRequestDto, CancellationToken)"/>
    public async Task<Result<Reserva, DomainError>> UpdateVehicleAsync(string codigoReferencia, ReservaUpdateVehicleRequestDto request, CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-UPDATE] Actualizando vehículo de la reserva {Codigo}.", codigoReferencia);

        var response = await _repository.UpdateVehicleAsync(codigoReferencia, request, ct);

        if (response is null)
            return Result.Failure<Reserva, DomainError>(ReservaErrors.NotFound(codigoReferencia))
                .Tap(_ => _logger.LogWarning("[SERVICE-UPDATE] No se encontró la reserva {Codigo}.", codigoReferencia));

        return Result.Success<Reserva, DomainError>(response)
            .Tap(r => _logger.LogInformation("[SERVICE-UPDATE] Vehículo de la reserva {Codigo} actualizado correctamente.", r.CodigoRefencia));
    }
    // =========================================================
    // EXISTS
    // =========================================================
    
    /// <inheritdoc cref="IReservaService.ExistsAsync(long, CancellationToken)"/>
    public async Task<Result<bool, DomainError>> ExistsAsync(long id, CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-EXISTS] Comprobando existencia de la reserva {Id}.", id);

        var response = await _repository.ExistsAsync(id, ct);

        return Result.Success<bool, DomainError>(response)
            .Tap(exists => _logger.LogDebug(
                "[SERVICE-EXISTS] La reserva {Id} {Estado}.",
                id, exists ? "existe" : "no existe"));
    }
}