using APIAlquilerVehiculos.Dtos.Email;
using APIAlquilerVehiculos.Dtos.Taller;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Errors.Taller;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Repositories.Taller;
using APIAlquilerVehiculos.Services.Email;
using CSharpFunctionalExtensions;

namespace APIAlquilerVehiculos.Services.Taller;

public class ParteService : IParteService
{
    private readonly IParteRepository _repository;
    private readonly INotificationEmailService _notificationEmail;
    private readonly ILogger<ParteService> _logger;

    private static readonly HashSet<Estado> EstadosReservaViva =
    [
        Estado.Pendiente,
        Estado.Confirmada,
        Estado.EnCurso
    ];

    public ParteService(
        IParteRepository repository,
        INotificationEmailService notificationEmail,
        ILogger<ParteService> logger)
    {
        _repository = repository;
        _notificationEmail = notificationEmail;
        _logger = logger;
    }

    public async Task<Result<List<ParteResponseDto>, DomainError>> GetAllAsync()
    {
        var partes = await _repository.GetAllAsync();
        var dtos = partes
            .Select(p => MapToDto(p, p.Vehiculo?.Matricula ?? string.Empty))
            .ToList();

        return Result.Success<List<ParteResponseDto>, DomainError>(dtos);
    }

    public async Task<Result<ParteResponseDto, DomainError>> GetByIdAsync(long id)
    {
        var parte = await _repository.GetByIdAsync(id);
        if (parte is null)
        {
            return Result.Failure<ParteResponseDto, DomainError>(
                NotFoundError.FromResource("Parte", id.ToString()));
        }

        return Result.Success<ParteResponseDto, DomainError>(
            MapToDto(parte, parte.Vehiculo?.Matricula ?? string.Empty));
    }

    public async Task<Result<ParteResponseDto, DomainError>> AbrirParteAsync(
        long trabajadorId,
        AbrirParteRequestDto request)
    {
        // 1. Verificar que es Mecánico activo (RF-08, RF-12)
        var trabajador = await _repository.GetTrabajadorByIdAsync(trabajadorId);
        if (trabajador is null || !trabajador.IsActivo || !EsMecanico(trabajador))
        {
            _logger.LogWarning("Trabajador {TrabajadorId} sin permiso intentó abrir parte", trabajadorId);
            return Result.Failure<ParteResponseDto, DomainError>(TallerErrors.SoloMecanicos());
        }

        // 2. Verificar estado del vehículo (RF-01, RF-05)
        var vehiculo = await _repository.GetVehiculoByIdAsync(request.VehiculoId);
        if (vehiculo is null)
        {
            return Result.Failure<ParteResponseDto, DomainError>(
                NotFoundError.FromResource("Vehículo", request.VehiculoId));
        }

        if (!vehiculo.Activo || vehiculo.Estado == EstadoEnum.Retirado)
        {
            return Result.Failure<ParteResponseDto, DomainError>(
                TallerErrors.VehiculoRetirado(vehiculo.Matricula));
        }

        if (vehiculo.Estado == EstadoEnum.EnMantenimiento || await _repository.TieneParteAbiertoAsync(vehiculo.Id))
        {
            return Result.Failure<ParteResponseDto, DomainError>(
                TallerErrors.ParteYaAbierto(vehiculo.Matricula));
        }

        // 3. Verificar que no tenga reservas vivas sin resolver (RF-07)
        var reservasVivas = (await _repository.GetReservasVivasByVehiculoIdAsync(vehiculo.Id))
            .Where(r => EstadosReservaViva.Contains(r.Estado))
            .ToList();

        if (reservasVivas.Count > 0)
        {
            var codigos = reservasVivas.Select(r => r.CodigoRefencia);
            return Result.Failure<ParteResponseDto, DomainError>(
                TallerErrors.VehiculoConReservasActivas(vehiculo.Matricula, codigos));
        }

        // 4. Crear el parte Abierto y pasar el coche a Mantenimiento (RF-08)
        var parte = new Parte
        {
            FechaApertura = DateTime.UtcNow,
            QuienLoTramitaId = trabajador.Id,
            Descripcion = request.Descripcion.Trim(),
            VehiculoId = vehiculo.Id,
            Vehiculo = vehiculo,
            Estado = EstadoParte.Abierto
        };

        vehiculo.Estado = EstadoEnum.EnMantenimiento;
        vehiculo.UpdatedAt = DateTime.UtcNow;

        var creado = await _repository.CreateAsync(parte);
        await _repository.UpdateVehiculoAsync(vehiculo);

        _logger.LogInformation("Parte {ParteId} abierto para vehículo {Matricula} por mecánico {TrabajadorId}",
            creado.Id, vehiculo.Matricula, trabajador.Id);

        return Result.Success<ParteResponseDto, DomainError>(MapToDto(creado, vehiculo.Matricula));
    }

    public async Task<Result<ParteResponseDto, DomainError>> CerrarParteAsync(
        long trabajadorId,
        long parteId)
    {
        var trabajador = await _repository.GetTrabajadorByIdAsync(trabajadorId);
        if (trabajador is null || !trabajador.IsActivo || !EsMecanico(trabajador))
        {
            return Result.Failure<ParteResponseDto, DomainError>(TallerErrors.SoloMecanicos());
        }

        var parte = await _repository.GetByIdAsync(parteId);
        if (parte is null)
        {
            return Result.Failure<ParteResponseDto, DomainError>(
                NotFoundError.FromResource("Parte", parteId.ToString()));
        }

        if (parte.Estado == EstadoParte.Cerrado)
        {
            return Result.Failure<ParteResponseDto, DomainError>(TallerErrors.ParteYaCerrado(parteId));
        }

        var vehiculo = await _repository.GetVehiculoByIdAsync(parte.VehiculoId);
        if (vehiculo is null)
        {
            return Result.Failure<ParteResponseDto, DomainError>(
                NotFoundError.FromResource("Vehículo", parte.VehiculoId));
        }

        // Cerrar parte, devolver coche a Disponible y actualizar UltimaRevision (RF-08)
        var ahora = DateTime.UtcNow;
        parte.Estado = EstadoParte.Cerrado;
        parte.FechaCierre = ahora;

        vehiculo.Estado = EstadoEnum.Disponible;
        vehiculo.UltimaRevision = ahora;
        vehiculo.UpdatedAt = ahora;

        await _repository.UpdateAsync(parte);
        await _repository.UpdateVehiculoAsync(vehiculo);

        _logger.LogInformation("Parte {ParteId} cerrado. Vehículo {Matricula} vuelve a Disponible",
            parte.Id, vehiculo.Matricula);

        return Result.Success<ParteResponseDto, DomainError>(MapToDto(parte, vehiculo.Matricula));
    }

    /// <summary>
    /// RF-07 / RF-34: Llamar justo después de guardar en BD la resolución de una reserva viva
    /// (cambio de vehículo equivalente, cancelación o aplazamiento) para meter el coche en taller.
    /// </summary>
    public async Task NotificarReservaAfectadaTallerAsync(
        string emailCliente,
        string nombreCliente,
        string codigoReferencia,
        Models.Vehiculo vehiculoOriginal,
        string descripcionAveria,
        string resolucionAplicada,
        string detalleResolucion)
    {
        if (string.IsNullOrWhiteSpace(emailCliente))
        {
            _logger.LogWarning(
                "No se pudo enviar correo de taller para reserva {Codigo}: cliente sin email.",
                codigoReferencia);
            return;
        }

        var nombreVehiculo = vehiculoOriginal.Marca is not null
            ? $"{vehiculoOriginal.Marca.Nombre} {vehiculoOriginal.Modelo} ({vehiculoOriginal.Matricula})"
            : $"{vehiculoOriginal.Modelo} ({vehiculoOriginal.Matricula})";

        await _notificationEmail.EnviarAsync(new ParteTallerEmailDto(
            EmailDestino: emailCliente,
            NombreCliente: nombreCliente,
            CodigoReferencia: codigoReferencia,
            VehiculoOriginal: nombreVehiculo,
            DescripcionAveria: descripcionAveria,
            ResolucionAplicada: resolucionAplicada,
            DetalleResolucion: detalleResolucion
        ));
    }

    private static bool EsMecanico(Trabajador trabajador) =>
        trabajador.TipoTrabajador.ToString().Equals("Mecanico", StringComparison.OrdinalIgnoreCase);

    private static ParteResponseDto MapToDto(Parte p, string matricula) =>
        new(p.Id, p.FechaApertura, p.FechaCierre, p.QuienLoTramitaId, p.Descripcion, p.VehiculoId, matricula, p.Estado);
}