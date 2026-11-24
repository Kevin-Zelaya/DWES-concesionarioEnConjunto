
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Errors.Reservas;
using APIAlquilerVehiculos.Extensions;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Repositories.Clientes;
using APIAlquilerVehiculos.Repositories.Vehiculos;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

public class ReservaService(
    IReservaRepository _repository,
    ILogger<ReservaService> _logger,
    IUnitOfWork _unitOfWork,
    ILineaReservaService _lineaService,
    ILineaReservaFactory _lineaReservaFactory,
    IDisponibilidadService _disponibilidadService,
    IVehiculosRepository _vehiculoRepository,
    IClienteRepository _clienteRepository
) : IReservaService
{
    // =========================================================
    // CREATE
    // =========================================================

    
    /// <inheritdoc cref="IReservaService.CreateReservaAsync(ReservaCreateRequestDto, long, CancellationToken)"/>
    public async Task<Result<ReservaDto, DomainError>> CreateReservaAsync(
        ReservaCreateRequestDto reserva,
        long TrabajadorId,
        CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-CREATE] Intentando crear una reserva.");

        // Comprobar que existe el cliente.
        var cliente = await _clienteRepository.FindByUserId(reserva.ClienteId);

        if (cliente is null)
        {
            return Result.Failure<ReservaDto, DomainError>(
                NotFoundError.FromResource("Cliente", reserva.ClienteId.ToString()));
        }

        // Comprobar que existe la oficina.
        var oficina = await _repository.GetOficinaById(1, ct);

        if (oficina is null)
        {
            return Result.Failure<ReservaDto, DomainError>(
                NotFoundError.FromResource("Oficina", "1"));
        }

        // Comprobar el horario de atención.
        if (!oficina.EstaDentroDelHorario(reserva.FechaRecogida) ||
            !oficina.EstaDentroDelHorario(reserva.FechaDevolucion))
        {
            return Result.Failure<ReservaDto, DomainError>(
                ReservaErrors.HorarioNoValido());
        }

        // Iniciar la transacción.
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // Bloquear el vehículo mientras se comprueba y genera la reserva.
            await _vehiculoRepository.BloquearVehiculoAsync(
                reserva.VehiculoId,
                ct);

            // Comprobar nuevamente la disponibilidad con el vehículo bloqueado.
            var disponibilidad = await _disponibilidadService.EstaDisponibleAsync(
                reserva.VehiculoId,
                reserva.FechaRecogida,
                reserva.FechaDevolucion,
                ct: ct);

            if (disponibilidad.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);

                return Result.Failure<ReservaDto, DomainError>(
                    disponibilidad.Error);
            }

            var vehiculo = disponibilidad.Value;

            // Factorizar las líneas de la reserva.
            var lineasFactorizadas = await FactorizarLineas(
                reserva.Lineas,
                vehiculo,
                Reserva.CalcularDiasAlquiler(
                    reserva.FechaRecogida,
                    reserva.FechaDevolucion),
                ct);

            if (lineasFactorizadas.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);

                return Result.Failure<ReservaDto, DomainError>(
                    lineasFactorizadas.Error);
            }

            // Crear la reserva.
            var reservaTS = reserva.ToEntity(TrabajadorId);
            // asignar el id del cliente
            reservaTS.ClienteId = cliente.Id;
            reservaTS = await _repository.CreateAsync(reservaTS, ct);

            // Asociar las líneas a la reserva creada.
            lineasFactorizadas.Value.ForEach(l =>
                l.ReservaId = reservaTS.Id);

            var lineasCreadas = await _lineaService.CreateLineasReservaAsync(
                lineasFactorizadas.Value,
                reservaTS.CalcularDiasAlquiler(),
                ct);

            if (lineasCreadas.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);

                return Result.Failure<ReservaDto, DomainError>(
                    lineasCreadas.Error);
            }

            // Calcular el precio total.
            reservaTS.PrecioTotal = _lineaService.CalculateTotalByLineas(
                lineasCreadas.Value);

            // Cambiar el estado del vehículo.
            vehiculo.Estado = EstadoEnum.Reservado;

            // Guardar y confirmar los cambios.
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            reservaTS.Lineas = lineasCreadas.Value;

            return Result.Success<ReservaDto, DomainError>(
                reservaTS.ToDto())
                .Tap(r => _logger.LogInformation(
                    "[SERVICE-CREATE] Reserva creada correctamente con ID {Id} y código {Codigo}.",
                    r.Id,
                    r.CodigoRefencia));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[SERVICE-CREATE] Error al crear la reserva, todos los cambios son eliminados.");

            await _unitOfWork.RollbackTransactionAsync(ct);

            throw;
        }
    }


    private async Task<Result<List<LineaReserva>, DomainError>> FactorizarLineas(
        List<LineaReservaCreateRequestDto> lineas,
        Vehiculo vehiculo,
        int dias,
        CancellationToken ct)
    {
        var lineasFd = new List<LineaReserva>
        {
            CrearLineaVehiculo(vehiculo, dias)
        };

        foreach (var linea in lineas)
        {
            var resultado = await _lineaReservaFactory.CreateAsync(
                linea,
                ct);

            if (resultado.IsFailure)
            {
                return Result.Failure<List<LineaReserva>, DomainError>(
                    resultado.Error);
            }

            lineasFd.Add(resultado.Value);
        }

        return Result.Success<List<LineaReserva>, DomainError>(
            lineasFd);
    }

    private static LineaReserva CrearLineaVehiculo(
        Vehiculo vehiculo,
        int dias)
    {
        return new LineaReserva
        {
            Tipo = TipoLinea.Vehiculo,
            Concepto = "Vehiculo con id" + vehiculo.Id,
            Cantidad = dias,
            PrecioUnitario = vehiculo.precioPorDia,
            TipoPrecio = TipoPrecio.Diario
        };
    }

    // =========================================================
    // READ
    // =========================================================

    /// <inheritdoc cref="IReservaService.GetReservaByIdAsync(long, CancellationToken)"/>
    public async Task<Result<Reserva, DomainError>> GetReservaByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-GET] Buscando reserva con ID {Id}.",
            id);

        var response = await _repository.GetByIdAsync(id, ct);

        if (response is null)
        {
            return Result.Failure<Reserva, DomainError>(
                ReservaErrors.NotFound(id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-GET] No se encontró la reserva con ID {Id}.",
                    id));
        }

        return Result.Success<Reserva, DomainError>(response)
            .Tap(r => _logger.LogInformation(
                "[SERVICE-GET] Reserva {Codigo} recuperada correctamente.",
                r.CodigoRefencia));
    }

    /// <inheritdoc cref="IReservaService.GetReservaByCodigoReferenciaAsync(string, CancellationToken)"/>
    public async Task<Result<Reserva, DomainError>> GetReservaByCodigoReferenciaAsync(
        string codigo,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-GET] Buscando reserva con código {Codigo}.",
            codigo);

        var response = await _repository.GetReservaByCodigoReferenciaAsync(
            codigo,
            ct);

        if (response is null)
        {
            return Result.Failure<Reserva, DomainError>(
                ReservaErrors.NotFound(codigo))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-GET] No se encontró la reserva con código {Codigo}.",
                    codigo));
        }

        return Result.Success<Reserva, DomainError>(response)
            .Tap(r => _logger.LogInformation(
                "[SERVICE-GET] Reserva {Codigo} recuperada correctamente.",
                r.CodigoRefencia));
    }

    /// <inheritdoc cref="IReservaService.GetAllReservasAsync(CancellationToken)"/>
    public async Task<Result<List<Reserva>, DomainError>> GetAllReservasAsync(
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-GETALL] Recuperando todas las reservas.");

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
    public async Task<Result<PaginatedResouce<Reserva>, DomainError>> GetPaginatedReservasAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct)
    {
        _logger.LogDebug(
            "[SERVICE-GETPAGINATED] Recuperando reservas. Página {PageNumber}, tamaño {PageSize}.",
            pageNumber,
            pageSize);

        var response = await _repository.GetPaginatedReservasAsync(
            pageNumber,
            pageSize,
            ct);

        var toResponse = new PaginatedResouce<Reserva>(
            response.items,
            response.count,
            response.totalPages,
            response.HasPreviousPage,
            response.HasNextPage);

        return Result.Success<PaginatedResouce<Reserva>, DomainError>(
            toResponse)
            .Tap(r => _logger.LogInformation(
                "[SERVICE-GETPAGINATED] Se recuperaron {Count} reservas de la página {PageNumber}.",
                r.Count,
                pageNumber));
    }

    /// <inheritdoc cref="IReservaService.GetPaginatedReservasByCustomerIdAsync(long, int, int, CancellationToken)"/>
    public async Task<Result<PaginatedResouce<Reserva>, DomainError>> GetPaginatedReservasByCustomerIdAsync(
        long id,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-GETPAGINATED] Recuperando reservas del cliente {Id}.",
            id);

        var response = await _repository.GetPaginatedReservasByCustomerIdAsync(
            id,
            pageNumber,
            pageSize,
            ct);

        var toResponse = new PaginatedResouce<Reserva>(
            response.items,
            response.count,
            response.totalPages,
            response.HasPreviousPage,
            response.HasNextPage);

        return Result.Success<PaginatedResouce<Reserva>, DomainError>(
            toResponse)
            .Tap(r => _logger.LogInformation(
                "[SERVICE-GETPAGINATED] Se recuperaron {Count} reservas del cliente {Id}.",
                r.Count,
                id));
    }

    /// <inheritdoc cref="IReservaService.GetPaginatedReservasByEmployeeIdAsync(long, int, int, CancellationToken)"/>
    public async Task<Result<PaginatedResouce<Reserva>, DomainError>> GetPaginatedReservasByEmployeeIdAsync(
        long id,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-GETPAGINATED] Recuperando reservas del trabajador {Id}.",
            id);

        var response = await _repository.GetPaginatedReservasByEmployeeIdAsync(
            id,
            pageNumber,
            pageSize,
            ct);

        var toResponse = new PaginatedResouce<Reserva>(
            response.items,
            response.count,
            response.totalPages,
            response.HasPreviousPage,
            response.HasNextPage);

        return Result.Success<PaginatedResouce<Reserva>, DomainError>(
            toResponse)
            .Tap(r => _logger.LogInformation(
                "[SERVICE-GETPAGINATED] Se recuperaron {Count} reservas del trabajador {Id}.",
                r.Count,
                id));
    }

    // =========================================================
    // UPDATE
    // =========================================================

    /// <inheritdoc cref="IReservaService.UpdateNotaAsync(string, ReservaUpdateNotaRequestDto, CancellationToken)"/>
    public async Task<Result<Reserva, DomainError>> UpdateNotaAsync(
        string codigoReferencia,
        ReservaUpdateNotaRequestDto request,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-UPDATE] Actualizando notas de la reserva {Codigo}.",
            codigoReferencia);

        var response = await _repository.UpdateNotaAsync(
            codigoReferencia,
            request,
            ct);

        if (response is null)
        {
            return Result.Failure<Reserva, DomainError>(
                ReservaErrors.NotFound(codigoReferencia))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-UPDATE] No se encontró la reserva {Codigo}.",
                    codigoReferencia));
        }

        return Result.Success<Reserva, DomainError>(response)
            .Tap(r => _logger.LogInformation(
                "[SERVICE-UPDATE] Notas de la reserva {Codigo} actualizadas correctamente.",
                r.CodigoRefencia));
    }
    
    public async Task<Result<Reserva, DomainError>> UpdateFechasAsync(
        string codigoReferencia,
        ReservaUpdateFechaRequestDto request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug(
                "[SERVICE-UPDATE] Actualizando fechas de la reserva {Codigo}.",
                codigoReferencia);

            var reserva = await _repository.GetReservaByCodigoReferenciaAsync(
                codigoReferencia, ct);

            if (reserva is null)
                return Result.Failure<Reserva, DomainError>(
                    ReservaErrors.NotFound(codigoReferencia));

            var oficina = await _repository.GetOficinaById(1, ct);

            if (oficina is null)
                return Result.Failure<Reserva, DomainError>(
                    NotFoundError.FromResource("Oficina", "1"));

            if (!oficina.EstaDentroDelHorario(request.FechaRecogida) ||
                !oficina.EstaDentroDelHorario(request.FechaDevolucion))
                return Result.Failure<Reserva, DomainError>(
                    ReservaErrors.HorarioNoValido());

            await _unitOfWork.BeginTransactionAsync(ct);

            try
            {
                await _vehiculoRepository.BloquearVehiculoAsync(
                    reserva.VehiculoId, ct);

                var disponibilidad = await _disponibilidadService.EstaDisponibleAsync(
                    reserva.VehiculoId,
                    request.FechaRecogida,
                    request.FechaDevolucion,
                    reserva.Id,
                    ct);

                if (disponibilidad.IsFailure)
                {
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result.Failure<Reserva, DomainError>(disponibilidad.Error);
                }

                var response = await _repository.UpdateFechasAsync(
                    codigoReferencia, request, ct);

                if (response is null)
                {
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result.Failure<Reserva, DomainError>(
                        ReservaErrors.NotFound(codigoReferencia));
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return Result.Success<Reserva, DomainError>(response)
                    .Tap(r => _logger.LogInformation(
                        "[SERVICE-UPDATE] Fechas de la reserva {Codigo} actualizadas correctamente.",
                        r.CodigoRefencia));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n========== ERROR EN LA TRANSACCIÓN ==========");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("==============================================\n");
                Console.ResetColor();

                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n========== ERROR EN UpdateFechasAsync ==========");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("================================================\n");
            Console.ResetColor();

            throw;
        }
    }

    /// <inheritdoc cref="IReservaService.UpdateStatusAsync(string, ReservaUpdateStatusRequestDto, CancellationToken)"/>
    public async Task<Result<Reserva, DomainError>> UpdateStatusAsync(
        string codigoReferencia,
        ReservaUpdateStatusRequestDto request,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-UPDATE] Actualizando estado de la reserva {Codigo}.",
            codigoReferencia);

        var reserva = await _repository.GetReservaByCodigoReferenciaAsync(
            codigoReferencia,
            ct);
        if (reserva is null)
        {
            return Result.Failure<Reserva, DomainError>(
                ReservaErrors.NotFound(codigoReferencia));
        }
        // Comprobar ssi se puede transicionar entre un estado y otro
        if (!reserva.Estado.PuedeTransicionarA(request.Estado))
        {
            return Result.Failure<Reserva, DomainError>(
                ReservaErrors.TransicionEstadoNoValida(
                    reserva.Estado,
                    request.Estado));
        }

        EstadoEnum? nuevoEstadoVehiculo = request.Estado switch
        {
            Estado.Confirmada => EstadoEnum.Reservado,
            Estado.EnCurso => EstadoEnum.EnAlquiler,
            Estado.Finalizada => EstadoEnum.Disponible,
            Estado.Cancelada => EstadoEnum.Disponible,
            _ => null
        };

        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // Bloquear el vehículo mientras se actualiza el estado.
            await _vehiculoRepository.BloquearVehiculoAsync(
                reserva.VehiculoId,
                ct);

            var response = await _repository.UpdateStatusAsync(
                codigoReferencia,
                request,
                ct);

            if (response is null)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);

                return Result.Failure<Reserva, DomainError>(
                    ReservaErrors.NotFound(codigoReferencia));
            }

            if (nuevoEstadoVehiculo.HasValue)
            {
                if (nuevoEstadoVehiculo == EstadoEnum.Disponible)
                {
                    var existenReservasActivas =
                        await _repository.ExistsReservasActivasAsync(
                            reserva.VehiculoId,
                            reserva.Id,
                            ct);

                    if (existenReservasActivas)
                    {
                        nuevoEstadoVehiculo = EstadoEnum.Reservado;
                    }
                }

                await _vehiculoRepository.UpdateEstado(
                    reserva.VehiculoId,
                    nuevoEstadoVehiculo.Value,
                    ct);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return Result.Success<Reserva, DomainError>(response)
                .Tap(r => _logger.LogInformation(
                    "[SERVICE-UPDATE] Estado de la reserva {Codigo} actualizado correctamente.",
                    r.CodigoRefencia));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[SERVICE-UPDATE] Error al actualizar el estado de la reserva {Codigo}.",
                codigoReferencia);

            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }


    /// <inheritdoc cref="IReservaService.UpdateVehicleAsync(string, ReservaUpdateVehicleRequestDto, CancellationToken)"/>
    public async Task<Result<Reserva, DomainError>> UpdateVehicleAsync(
        string codigoReferencia,
        ReservaUpdateVehicleRequestDto request,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-UPDATE] Actualizando vehículo de la reserva {Codigo}.",
            codigoReferencia);

        var reserva = await _repository.GetReservaByCodigoReferenciaAsync(
            codigoReferencia,
            ct);

        if (reserva is null)
        {
            return Result.Failure<Reserva, DomainError>(
                ReservaErrors.NotFound(codigoReferencia));
        }

        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // Bloquear el nuevo vehículo.
            await _vehiculoRepository.BloquearVehiculoAsync(
                request.VehiculoId,
                ct);

            // Comprobar nuevamente su disponibilidad.
            var disponibilidad = await _disponibilidadService.EstaDisponibleAsync(
                request.VehiculoId,
                reserva.FechaRecogida,
                reserva.FechaDevolucion,
                reserva.Id,
                ct);

            if (disponibilidad.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);

                return Result.Failure<Reserva, DomainError>(
                    disponibilidad.Error);
            }

            // Actualizar el vehículo de la reserva.
            var response = await _repository.UpdateVehicleAsync(
                codigoReferencia,
                request,
                ct);

            if (response is null)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);

                return Result.Failure<Reserva, DomainError>(
                    ReservaErrors.NotFound(codigoReferencia));
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return Result.Success<Reserva, DomainError>(response)
                .Tap(r => _logger.LogInformation(
                    "[SERVICE-UPDATE] Vehículo de la reserva {Codigo} actualizado correctamente.",
                    r.CodigoRefencia));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[SERVICE-UPDATE] Error al actualizar el vehículo de la reserva {Codigo}.",
                codigoReferencia);

            await _unitOfWork.RollbackTransactionAsync(ct);

            throw;
        }
    }


    // =========================================================
    // EXISTS
    // =========================================================

    /// <inheritdoc cref="IReservaService.ExistsAsync(long, CancellationToken)"/>
    public async Task<Result<bool, DomainError>> ExistsAsync(
        long id,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-EXISTS] Comprobando existencia de la reserva {Id}.",
            id);

        var response = await _repository.ExistsAsync(id, ct);

        return Result.Success<bool, DomainError>(response)
            .Tap(exists => _logger.LogDebug(
                "[SERVICE-EXISTS] La reserva {Id} {Estado}.",
                id,
                exists ? "existe" : "no existe"));
    }
}
