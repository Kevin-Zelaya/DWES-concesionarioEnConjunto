using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using CSharpFunctionalExtensions;

public interface IDisponibilidadService
{
    Task<Result<List<Vehiculo>, DomainError>>
        GetVehiculosDisponiblesPorCategoriaAsync(
            CategoriaEnum categoria,
            DateTime fechaRecogida,
            DateTime fechaDevolucion,
            CancellationToken ct = default);

    Task<Result<Vehiculo, DomainError>>
        GetDisponibilidadPorMatriculaAsync(
            string matricula,
            DateTime fechaRecogida,
            DateTime fechaDevolucion,
            CancellationToken ct = default);

    Task<Result<Vehiculo, DomainError>> EstaDisponibleAsync(
        string vehiculoId,
        DateTime fechaRecogida,
        DateTime fechaDevolucion,
        long? reservaIdExcluir = null,
        CancellationToken ct = default);
}