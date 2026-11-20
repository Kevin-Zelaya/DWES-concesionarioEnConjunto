
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Repositories.Vehiculos;
using CSharpFunctionalExtensions;

public class DisponibilidadService(
    IVehiculosRepository _vehiculoRepository,
    IReservaRepository _reservaRepository
) : IDisponibilidadService
{
    public async Task<Result<Vehiculo, DomainError>> EstaDisponibleAsync(
        string vehiculoId,
        DateTime fechaRecogida,
        DateTime fechaDevolucion,
        CancellationToken ct = default)
    {
        if (!FechasValidas(fechaRecogida, fechaDevolucion))
        {
            return Result.Failure<Vehiculo, DomainError>(
                DisponibilidadErrors.FechasNoValidas());
        }

        var vehiculo = await _vehiculoRepository.GetByIdAsync(vehiculoId);

        if (vehiculo is null)
        {
            return Result.Failure<Vehiculo, DomainError>(
                DisponibilidadErrors.VehiculoNotFound(vehiculoId));
        }

        var tieneReserva = await _reservaRepository.ExistsReservaSolapadaAsync(
            vehiculoId,
            fechaRecogida,
            fechaDevolucion,
            ct);

        if (tieneReserva)
        {
            return Result.Failure<Vehiculo, DomainError>(
                DisponibilidadErrors.VehiculoNoDisponible(vehiculoId));
        }

        return Result.Success<Vehiculo, DomainError>(vehiculo);
    }

    public async Task<Result<Vehiculo, DomainError>> GetDisponibilidadPorMatriculaAsync(
        string matricula,
        DateTime fechaRecogida,
        DateTime fechaDevolucion,
        CancellationToken ct = default)
    {
        if (!FechasValidas(fechaRecogida, fechaDevolucion))
        {
            return Result.Failure<Vehiculo, DomainError>(
                DisponibilidadErrors.FechasNoValidas());
        }

        var vehiculo = await _vehiculoRepository.GetByMatriculaAsync(matricula);

        if (vehiculo is null)
        {
            return Result.Failure<Vehiculo, DomainError>(
                DisponibilidadErrors.VehiculoNotFound(matricula));
        }

        var tieneReserva = await _reservaRepository.ExistsReservaSolapadaAsync(
            vehiculo.Id,
            fechaRecogida,
            fechaDevolucion,
            ct);

        if (tieneReserva)
        {
            return Result.Failure<Vehiculo, DomainError>(
                DisponibilidadErrors.VehiculoNoDisponible(matricula));
        }

        return Result.Success<Vehiculo, DomainError>(vehiculo);
    }

    public async Task<Result<List<Vehiculo>, DomainError>>
        GetVehiculosDisponiblesPorCategoriaAsync(
            CategoriaEnum categoria,
            DateTime fechaRecogida,
            DateTime fechaDevolucion,
            CancellationToken ct = default)
    {
        if (!FechasValidas(fechaRecogida, fechaDevolucion))
        {
            return Result.Failure<List<Vehiculo>, DomainError>(
                DisponibilidadErrors.FechasNoValidas());
        }

        var vehiculos = await _vehiculoRepository.GetByCategoriaAsync(
            categoria, ct);

        var disponibles = new List<Vehiculo>();

        foreach (var vehiculo in vehiculos)
        {
            var tieneReserva = await _reservaRepository.ExistsReservaSolapadaAsync(
                vehiculo.Id,
                fechaRecogida,
                fechaDevolucion,
                ct);

            if (!tieneReserva)
            {
                disponibles.Add(vehiculo);
            }
        }

        return Result.Success<List<Vehiculo>, DomainError>(disponibles);
    }

    private static bool FechasValidas(
        DateTime fechaRecogida,
        DateTime fechaDevolucion)
    {
        return fechaRecogida < fechaDevolucion;
    }
}
