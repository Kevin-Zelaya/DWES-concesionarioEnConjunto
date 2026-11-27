
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Errors.Reservas;
using APIAlquilerVehiculos.Extensions;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Repositories.Clientes;
using APIAlquilerVehiculos.Repositories.Vehiculos;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using APIAlquilerVehiculos.Entities.Cassandra;
using APIAlquilerVehiculos.Services.Cassandra;
using System.Linq;

/// <summary>
/// Implementación de <see cref="IReservaService"/> para gestionar las reservas.
/// </summary>
public class ReservaService(
    IReservaRepository _repository,
    ILogger<ReservaService> _logger,
    IUnitOfWork _unitOfWork,
    ILineaReservaService _lineaService,
    ILineaReservaFactory _lineaReservaFactory,
    IDisponibilidadService _disponibilidadService,
    IVehiculosRepository _vehiculoRepository,
    IClienteRepository _clienteRepository,
    IncidenciaService _incidenciaService,
    IRecargoRepository _recargoRepository,
    INotificationService _notificationService,
    IReservaEventoService _reservaEventoService

) : IReservaService
{
    // =========================================================
    // CREATE
    // =========================================================

    
    /// <inheritdoc cref="IReservaService.CreateReservaAsync(ReservaCreateRequestDto, long, CancellationToken)"/>
    public async Task<Result<ReservaDto, DomainError>> CreateReservaAsync(ReservaCreateRequestDto reserva, long TrabajadorId, CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-CREATE] Intentando crear una reserva.");
        // para compatiblidad con fechas especificas transformamos los datatime 
        var fechaRecogida= DateTime.SpecifyKind(reserva.FechaRecogida, DateTimeKind.Local).ToUniversalTime();
        var fechaDevolucion = DateTime.SpecifyKind(reserva.FechaDevolucion, DateTimeKind.Local).ToUniversalTime();
        // Comprobamos si existe el cliente
        var cliente = await _clienteRepository.FindByUserId(reserva.ClienteId, ct);

        if (cliente is null)
            return Result.Failure<ReservaDto, DomainError>(
                NotFoundError.FromResource("Cliente", reserva.ClienteId.ToString()));

        // Comprobamos si existe la oficina
        var oficina = await _repository.GetOficinaById(1, ct); // temporal 

        if (oficina is null)
            return Result.Failure<ReservaDto, DomainError>(
                NotFoundError.FromResource("Oficina", "1"));

        // Comprobamos que las fechas estén dentro del horario de atención
        if (!oficina.EstaDentroDelHorario(fechaRecogida) ||
            !oficina.EstaDentroDelHorario(fechaDevolucion))
            return Result.Failure<ReservaDto, DomainError>(
                ReservaErrors.HorarioNoValido());

        // Iniciamos la transacción
        await _unitOfWork.BeginTransactionAsync(ct);
        // Mapear ek request a entidad
        var reservaTS = reserva.ToEntity(TrabajadorId);
        reservaTS.ClienteId = cliente.Id;
        try
        {
            // Bloqueamos el vehículo para evitar modificaciones simultáneas
            await _vehiculoRepository.BloquearVehiculoAsync(reserva.VehiculoId, ct);

            // Comprobamos si el vehículo está disponible para las fechas indicadas
            var disponibilidad = await _disponibilidadService.EstaDisponibleAsync(
                reservaTS.VehiculoId,
                reservaTS.FechaRecogida,
                reservaTS.FechaDevolucion,
                ct: ct);

            if (disponibilidad.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return Result.Failure<ReservaDto, DomainError>(disponibilidad.Error);
            }

            var vehiculo = disponibilidad.Value;

            // Generamos las líneas de la reserva
            var lineasFactorizadas = await FactorizarLineas(
                reserva.Lineas,
                vehiculo,
                Reserva.CalcularDiasAlquiler(reserva.FechaRecogida, reserva.FechaDevolucion),
                cliente,
                ct);

            if (lineasFactorizadas.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return Result.Failure<ReservaDto, DomainError>(lineasFactorizadas.Error);
            }
            // fecha en la que caduca el pago: dentro de 30 minutos
            reservaTS.FechaExpiracionPago = DateTime.UtcNow.AddMinutes(30);
            // Creamos la reserva 
            reservaTS = await _repository.CreateAsync(reservaTS, ct);

            // Asociamos las líneas a la reserva creada
            lineasFactorizadas.Value.ForEach(l => l.ReservaId = reservaTS.Id);

            // Guardamos las líneas de la reserva
            var lineasCreadas = await _lineaService.CreateLineasReservaAsync(
                lineasFactorizadas.Value,
                reservaTS.CalcularDiasAlquiler(),
                ct);

            if (lineasCreadas.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return Result.Failure<ReservaDto, DomainError>(lineasCreadas.Error);
            }

            // Calculamos el precio total de la reserva
            reservaTS.PrecioTotal = _lineaService.CalculateTotalByLineas(lineasCreadas.Value);

            // Actualizamos el estado del vehículo
            vehiculo.Estado = EstadoEnum.Reservado;
           

            // Guardamos los cambios y confirmamos la transacción
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
             
             // Registrar evento de reserva creada en Cassandra.
            await _reservaEventoService.RegistrarEventoAsync(new ReservaEvento
            {
                EventoId = Guid.NewGuid(),
                ReservaId = reservaTS.Id,
                CodigoReferencia = reservaTS.CodigoRefencia ?? string.Empty,
                TipoEvento = "ReservaCreada",
                FechaEvento = DateTime.UtcNow,
                VehiculoId = vehiculo.Id,
                Modelo = vehiculo.Modelo,
                Categoria = vehiculo.categoria.ToString(),
                Polizas = lineasCreadas.Value
                    .Where(l => l.Tipo == TipoLinea.Poliza)
                    .Select(l => l.Concepto)
                    .ToList(),
                FechaRecogida = reservaTS.FechaRecogida,
                FechaDevolucion = reservaTS.FechaDevolucion
            });

            // Asociamos las líneas y devolvemos el resultado
            reservaTS.Lineas = lineasCreadas.Value;

            return Result.Success<ReservaDto, DomainError>(reservaTS.ToDto())
                .Tap(r => _logger.LogInformation(
                    "[SERVICE-CREATE] Reserva creada correctamente con ID {Id} y código {Codigo}.",
                    r.Id,
                    r.CodigoRefencia))
                // Notificaciones
                .Tap(f => {
                        _notificationService.NotifyUserAsync(
                            cliente.IdUsuario.ToString(),
                            new ResourceNotification<string>
                            {
                                Resource = "Reserva",
                                Action = "Creada",
                                ResourceId = reservaTS.CodigoRefencia,
                                Message = "Tienes una nueva reserva pendiente de pago",
                                Data = new
                                {
                                    reservaTS.FechaRecogida,
                                    reservaTS.FechaDevolucion,
                                    vehiculo.Matricula,
                                    reservaTS.Estado
                                },
                                CreatedAt = reservaTS.FechaCreacion
                            });});

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

    /// <summary>
    /// Genera las líneas de reserva a partir de los datos recibidos, incluyendo la línea correspondiente al vehículo.
    /// </summary>
    /// <param name="lineas">Líneas adicionales solicitadas para la reserva.</param>
    /// <param name="vehiculo">Vehículo asociado a la reserva.</param>
    /// <param name="dias">Número de días de alquiler.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Resultado con las líneas generadas o un error de dominio.</returns>
    private async Task<Result<List<LineaReserva>, DomainError>> FactorizarLineas(
        List<LineaReservaCreateRequestDto> lineas,
        Vehiculo vehiculo,
        int dias,
        Cliente cliente,
        CancellationToken ct)
    {
        var lineasFd = new List<LineaReserva>
        {
            CrearLineaVehiculo(vehiculo, dias)
        };
        if (cliente.CalcularEdad() < 25)
        {
            var lj = await CrearLineaJoven(vehiculo.precioPorDia, ct);
            lineasFd.Add(lj);
        }

        foreach (var linea in lineas)
        {   
            // como las lineas de vehículo y recargo por jogen las hacemo nosotros las sltamos
            if(linea.Tipo == TipoLinea.Vehiculo)
                continue;
            if(linea.Tipo == TipoLinea.Recargo && linea.TipoId == 1)
                continue;

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

        return Result.Success<List<LineaReserva>, DomainError>(lineasFd);
    }
    /// <summary>
    /// Crea la línea correspondiente al alquiler del vehículo.
    /// </summary>
    /// <param name="vehiculo">Vehículo asociado a la reserva.</param>
    /// <param name="dias">Número de días de alquiler.</param>
    /// <returns>Línea de reserva con el precio diario del vehículo.</returns>
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
    private async Task<LineaReserva> CrearLineaJoven(
        decimal precioVehículo,
        CancellationToken ct)
    {
        var recargo = await _recargoRepository.GetByIdAsync(1, ct);

        return new LineaReserva
        {
            Tipo = TipoLinea.Recargo,
            Concepto = recargo?.Nombre ?? "Recargo conductor joven",
            Cantidad = 1,
            PrecioUnitario = recargo is not null 
                ?precioVehículo*(recargo!.Importe/100) // El importe es realmente el porcentaje que se le aplica al precio
                :precioVehículo*0.25m, // por ejemplo si el precio es 80 sería. 80*0.25 = 20 €
            TipoPrecio = recargo?.TipoPrecio ?? TipoPrecio.Diario // el precio se define como diario, ya que es un importe que aumente un 25% (por ejemplo) al precio del vehículo
            // y el precio del vehículo se calcula por el número de días
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
    public async Task<Result<List<ReservaDto>, DomainError>> GetAllReservasAsync(
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-GETALL] Recuperando todas las reservas.");

        var response = await _repository.GetAllAsync(ct);

        var reservas = response.Select(r => r.ToDto()).ToList();

        return Result.Success<List<ReservaDto>, DomainError>(reservas)
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

   /// <inheritdoc cref="IReservaService.UpdateFechasAsync(string, ReservaUpdateFechaRequestDto, CancellationToken)"/>
    public async Task<Result<Reserva, DomainError>> UpdateFechasAsync(string codigoReferencia, ReservaUpdateFechaRequestDto request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("[SERVICE-UPDATE] Actualizando fechas de la reserva {Codigo}.", codigoReferencia);

            // Comprobamos si existe la reserva
            var reserva = await _repository.GetReservaByCodigoReferenciaAsync(codigoReferencia, ct);

            if (reserva is null)
                return Result.Failure<Reserva, DomainError>(ReservaErrors.NotFound(codigoReferencia));

            // Obtenemos la oficina
            var oficina = await _repository.GetOficinaById(1, ct);

            if (oficina is null)
                return Result.Failure<Reserva, DomainError>(NotFoundError.FromResource("Oficina", "1"));

            // Comprobamos que las fechas estén dentro del horario de la oficina
            if (!oficina.EstaDentroDelHorario(request.FechaRecogida) ||
                !oficina.EstaDentroDelHorario(request.FechaDevolucion))
                return Result.Failure<Reserva, DomainError>(ReservaErrors.HorarioNoValido());

            // Iniciamos la transacción
            await _unitOfWork.BeginTransactionAsync(ct);

            // Bloqueamos el vehículo para evitar modificaciones simultáneas
            await _vehiculoRepository.BloquearVehiculoAsync(reserva.VehiculoId, ct);

            // Comprobamos que el vehículo esté disponible para las nuevas fechas
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

            // Actualizamos las fechas de la reserva
            var response = await _repository.UpdateFechasAsync(codigoReferencia, request, ct);

            if (response is null)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return Result.Failure<Reserva, DomainError>(ReservaErrors.NotFound(codigoReferencia));
            }

            // Guardamos los cambios y confirmamos la transacción
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return Result.Success<Reserva, DomainError>(response)
                .Tap(r => _logger.LogInformation(
                    "[SERVICE-UPDATE] Fechas de la reserva {Codigo} actualizadas correctamente.",
                    r.CodigoRefencia));
        }
        catch (Exception ex)
        {
            // Si ocurre un error, deshacemos los cambios
            _logger.LogError(ex, "[SERVICE-UPDATE] Error al actualizar las fechas de la reserva {Codigo}.", codigoReferencia);

            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    /// <inheritdoc cref="IReservaService.UpdateStatusAsync(string, ReservaUpdateStatusRequestDto, CancellationToken)"/>
    public async Task<Result<Reserva, DomainError>> UpdateStatusAsync(
        string codigoReferencia,
        ReservaUpdateStatusRequestDto request,
        CancellationToken ct = default)
    {
        _logger.LogDebug("[SERVICE-UPDATE] Actualizando estado de la reserva {Codigo}.",
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

                await _vehiculoRepository.UpdateEstadoAsync(
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

    
    /// <inheritdoc cref="IReservaService.ConfirmarRecogidaAsync(string, CancellationToken)"/>
    public async Task<Result<Reserva, DomainError>> ConfirmarRecogidaAsync(string codigoReferencia, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-CONFIRMAR-RECOGIDA] Confirmando recogida de la reserva {Codigo}.",
            codigoReferencia);

        var reserva = await _repository.GetReservaByCodigoReferenciaAsync(
            codigoReferencia,
            ct);

        if (reserva is null)
            return Result.Failure<Reserva, DomainError>(
                ReservaErrors.NotFound(codigoReferencia));

        // Comprobar si se puede realizar la transición a EnCurso.
        if (!reserva.Estado.PuedeTransicionarA(Estado.EnCurso))
            return Result.Failure<Reserva, DomainError>(
                ReservaErrors.TransicionEstadoNoValida(
                    reserva.Estado,
                    Estado.EnCurso));

        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // Bloquear el vehículo mientras se actualiza la reserva.
            await _vehiculoRepository.BloquearVehiculoAsync(
                reserva.VehiculoId,
                ct);

            var response = await _repository.UpdateStatusAsync(
                codigoReferencia,
                new ReservaUpdateStatusRequestDto(
                    Estado.EnCurso,
                    reserva.RowVersion),
                ct);

            if (response is null)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);

                return Result.Failure<Reserva, DomainError>(
                    ReservaErrors.NotFound(codigoReferencia));
            }

            // Actualizar el estado del vehículo a EnAlquiler.
            await _vehiculoRepository.UpdateEstadoAsync(
                reserva.VehiculoId,
                EstadoEnum.EnAlquiler,
                ct);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return Result.Success<Reserva, DomainError>(response)
                .Tap(r => _logger.LogInformation(
                    "[SERVICE-CONFIRMAR-RECOGIDA] Recogida confirmada correctamente para la reserva {Codigo}.",
                    r.CodigoRefencia));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[SERVICE-CONFIRMAR-RECOGIDA] Error al confirmar la recogida de la reserva {Codigo}.",
                codigoReferencia);

            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }   

    
    /// <inheritdoc cref="IReservaService.ConfirmarDevolucionAsync(string, CancellationToken)"/>
    public async Task<Result<Reserva, DomainError>> ConfirmarDevolucionAsync(string codigoReferencia, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-CONFIRMAR-DEVOLUCION] Confirmando devolución de la reserva {Codigo}.",
            codigoReferencia);

        var reserva = await _repository.GetReservaByCodigoReferenciaAsync(
            codigoReferencia,
            ct);

        if (reserva is null)
            return Result.Failure<Reserva, DomainError>(
                ReservaErrors.NotFound(codigoReferencia));

        // Comprobar si se puede realizar la transición a Finalizada.
        if (!reserva.Estado.PuedeTransicionarA(Estado.Finalizada))
            return Result.Failure<Reserva, DomainError>(
                ReservaErrors.TransicionEstadoNoValida(
                    reserva.Estado,
                    Estado.Finalizada));

        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // Bloquear el vehículo mientras se actualiza la reserva.
            await _vehiculoRepository.BloquearVehiculoAsync(
                reserva.VehiculoId,
                ct);

            var response = await _repository.UpdateStatusAsync(
                codigoReferencia,
                new ReservaUpdateStatusRequestDto(
                    Estado.Finalizada,
                    reserva.RowVersion),
                ct);

            if (response is null)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);

                return Result.Failure<Reserva, DomainError>(
                    ReservaErrors.NotFound(codigoReferencia));
            }

            // Comprobar si existen otras reservas activas para el vehículo.
            var existenReservasActivas = await _repository.ExistsReservasActivasAsync(
                reserva.VehiculoId,
                reserva.Id,
                ct);

            // Actualizar el estado del vehículo según las reservas existentes.
            var nuevoEstadoVehiculo = existenReservasActivas
                ? EstadoEnum.Reservado
                : EstadoEnum.Disponible;

            await _vehiculoRepository.UpdateEstadoAsync(
                reserva.VehiculoId,
                nuevoEstadoVehiculo,
                ct);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return Result.Success<Reserva, DomainError>(response)
                .Tap(r => _logger.LogInformation(
                    "[SERVICE-CONFIRMAR-DEVOLUCION] Devolución confirmada correctamente para la reserva {Codigo}.",
                    r.CodigoRefencia));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[SERVICE-CONFIRMAR-DEVOLUCION] Error al confirmar la devolución de la reserva {Codigo}.",
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
        _logger.LogDebug("[SERVICE-EXISTS] Comprobando existencia de la reserva {Id}.", id);

        var response = await _repository.ExistsAsync(id, ct);

        return Result.Success<bool, DomainError>(response)
            .Tap(exists => _logger.LogDebug(
                "[SERVICE-EXISTS] La reserva {Id} {Estado}.",
                id,
                exists ? "existe" : "no existe"));
    }

    /// <summary>
    /// Procesa las reservas confirmadas cuyo día de recogida ha finalizado.
    /// Las marca como NoPresentado y libera el vehículo si no tiene otras reservas activas.
    /// </summary>
    /// <inheritdoc cref="IReservaService.ProcesarReservasPendientesAsync(CancellationToken)"/>
    public async Task ProcesarReservasPendientesAsync(
        CancellationToken ct = default)
    {
        _logger.LogInformation("[BACKGROUND-RESERVAS] Iniciando comprobación de reservas pendientes.");

        // Obtener el comienzo del día actual en Madrid.
        var zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById(
            "Europe/Madrid");

        var ahoraMadrid = TimeZoneInfo.ConvertTime(
            DateTimeOffset.UtcNow,
            zonaHoraria);

        var inicioDiaMadrid = new DateTimeOffset(
            ahoraMadrid.Year,
            ahoraMadrid.Month,
            ahoraMadrid.Day,
            0, 0, 0,
            ahoraMadrid.Offset);

        // Convertir el comienzo del día a UTC.
        var inicioDiaUtc = inicioDiaMadrid
            .ToUniversalTime()
            .UtcDateTime;

        var reservas = await _repository.GetReservasNoPresentadasAsync(
            inicioDiaUtc,
            ct);

        if (reservas.Count == 0)
        {
            _logger.LogDebug(
                "[BACKGROUND-RESERVAS] No hay reservas pendientes de procesar.");

            return;
        }

        foreach (var reserva in reservas)
        {
            ct.ThrowIfCancellationRequested();

            await _unitOfWork.BeginTransactionAsync(ct);

            try
            {
                // Bloquear el vehículo.
                await _vehiculoRepository.BloquearVehiculoAsync(
                    reserva.VehiculoId,
                    ct);

                // Recuperar nuevamente la reserva.
                var reservaActual = await _repository.GetByIdAsync(
                    reserva.Id,
                    ct);

                if (reservaActual is null ||
                    reservaActual.FechaRecogida >= inicioDiaUtc ||
                    !reservaActual.Estado.PuedeTransicionarA(Estado.NoPresentado))
                {
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    continue;
                }

                // Marcar la reserva como no presentada.
                reservaActual.Estado = Estado.NoPresentado;

                // Comprobar si el vehículo está reservado.
                if (reservaActual.Vehiculo is not null &&
                    reservaActual.Vehiculo.Estado == EstadoEnum.Reservado)
                {
                    var existenReservasActivas =
                        await _repository.ExistsReservasActivasAsync(
                            reservaActual.VehiculoId,
                            reservaActual.Id,
                            ct);

                    var nuevoEstadoVehiculo = existenReservasActivas
                        ? EstadoEnum.Reservado
                        : EstadoEnum.Disponible;

                    if (reservaActual.Vehiculo.Estado.PuedeTransicionarA(
                        nuevoEstadoVehiculo))
                    {
                        await _vehiculoRepository.UpdateEstadoAsync(
                            reservaActual.VehiculoId,
                            nuevoEstadoVehiculo,
                            ct);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("[BACKGROUND-RESERVAS] Reserva {Codigo} marcada como NoPresentado.",
                    reservaActual.CodigoRefencia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BACKGROUND-RESERVAS] Error al procesar la reserva {Id}.",
                    reserva.Id);

                await _unitOfWork.RollbackTransactionAsync(ct);
            }
        }
    }
    /// <inheritdoc cref="IReservaService.ProcesarDevolucionesTardiasAsync(CancellationToken)"/>
    public async Task ProcesarDevolucionesTardiasAsync(
        CancellationToken ct = default)
    {
        var ahoraUtc = DateTime.UtcNow;

        var reservas = await _repository.GetReservasConDevolucionTardiaAsync(
            ahoraUtc, ct);

        foreach (var reserva in reservas)
        {
            var incidencia = new IncidenciaReserva(
                reserva.Id,
                reserva.CodigoRefencia ?? string.Empty,
                reserva.ClienteId,
                reserva.VehiculoId,
                TipoIncidencia.DevolucionTardia,
                "El vehículo no ha sido devuelto en la fecha prevista.",
                ahoraUtc
            );

            _incidenciaService.Add(incidencia);
        }
    }
    /// <inheritdoc cref="IReservaService.ProcesarVehiculosNoDisponiblesAsync(CancellationToken)"/>
    public async Task ProcesarVehiculosNoDisponiblesAsync(
        CancellationToken ct = default)
    {
        var ahora = DateTime.UtcNow;

        var reservas = await _repository
            .GetReservasConVehiculoNoDisponibleAsync(
                ahora,
                ahora.AddHours(3),
                ct);

        foreach (var reserva in reservas)
        {
            var incidencia = new IncidenciaReserva(
                reserva.Id,
                reserva.CodigoRefencia ?? string.Empty,
                reserva.ClienteId,
                reserva.VehiculoId,
                TipoIncidencia.VehiculoNoDisponible,
                "El vehículo reservado todavía se encuentra en alquiler."
            );

            _incidenciaService.Add(incidencia);
        }
    }

    /// <inheritdoc cref="IReservaService.ProcesarReservasPendientesAsync(CancellationToken)"/>
    public async Task ProcesarReservasPendientesDePagoAsync(
        CancellationToken ct = default)
    {
        var reservas = await _repository.GetReservasPendientesDePagoAsync(ct);

        var ahora = DateTime.UtcNow;

        // Primero se obtienen todas las wque esten caducadas
        var reservasCaducadas = reservas
            .Where(r => r.FechaExpiracionPago <= ahora)
            .ToList();

        if (reservasCaducadas.Count == 0)
            return;

        // actualizamos el estado de las reservas.
        foreach (var reserva in reservasCaducadas)
        {
            reserva.Estado = Estado.Caducada;
        }
        // Con todas se guarda
        await _unitOfWork.SaveChangesAsync(ct);
        // notificamos a los clientes de las reservas caducadas.
        foreach (var reserva in reservasCaducadas)
        {
            await _notificationService.NotifyUserAsync(
                reserva.ClienteId.ToString(),
                new ResourceNotification<string>
                {
                    Resource = "Reserva",
                    Action = "Caducada",
                    ResourceId = reserva.CodigoRefencia!,
                    Message = "Tu reserva ha caducado porque ha expirado el tiempo de pago.",
                    Data = new
                    {
                        reserva.CodigoRefencia,
                        reserva.FechaRecogida,
                        reserva.Vehiculo!.Matricula,
                        reserva.Estado
                    },
                    CreatedAt = ahora
                });

        }
    }
}
