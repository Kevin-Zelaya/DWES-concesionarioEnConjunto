using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using CSharpFunctionalExtensions;


/// <summary>
/// Define las operaciones para consultar la disponibilidad de los vehículos
/// en función de sus fechas de recogida y devolución, matrícula o categoría.
/// </summary>
public interface IDisponibilidadService
{
    /// <summary>
    /// Obtiene los vehículos disponibles de una categoría para un intervalo de fechas determinado.
    /// </summary>
    /// <param name="categoria">Categoría de los vehículos que se desean consultar.</param>
    /// <param name="fechaRecogida">Fecha de inicio del alquiler.</param>
    /// <param name="fechaDevolucion">Fecha de finalización del alquiler.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>
    /// Una lista de vehículos disponibles o un error de dominio si las fechas no son válidas.
    /// </returns>
    Task<Result<List<Vehiculo>, DomainError>> GetVehiculosDisponiblesPorCategoriaAsync(
        CategoriaEnum categoria,
        DateTime fechaRecogida,
        DateTime fechaDevolucion,
        CancellationToken ct = default);

    /// <summary>
    /// Comprueba la disponibilidad de un vehículo a partir de su matrícula
    /// para un intervalo de fechas determinado.
    /// </summary>
    /// <param name="matricula">Matrícula del vehículo que se desea consultar.</param>
    /// <param name="fechaRecogida">Fecha de inicio del alquiler.</param>
    /// <param name="fechaDevolucion">Fecha de finalización del alquiler.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>
    /// El vehículo si está disponible o un error de dominio si no existe,
    /// las fechas no son válidas o tiene una reserva solapada.
    /// </returns>
    Task<Result<Vehiculo, DomainError>> GetDisponibilidadPorMatriculaAsync(
        string matricula,
        DateTime fechaRecogida,
        DateTime fechaDevolucion,
        CancellationToken ct = default);

    /// <summary>
    /// Comprueba si un vehículo está disponible para un intervalo de fechas determinado.
    /// Permite excluir una reserva existente al modificar sus fechas o su vehículo.
    /// </summary>
    /// <param name="vehiculoId">Identificador del vehículo que se desea consultar.</param>
    /// <param name="fechaRecogida">Fecha de inicio del alquiler.</param>
    /// <param name="fechaDevolucion">Fecha de finalización del alquiler.</param>
    /// <param name="reservaIdExcluir">Identificador de la reserva que se debe excluir de la comprobación, si procede.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>
    /// El vehículo si está disponible o un error de dominio si no existe,
    /// las fechas no son válidas o tiene una reserva solapada.
    /// </returns>
    Task<Result<Vehiculo, DomainError>> EstaDisponibleAsync(
        string vehiculoId,
        DateTime fechaRecogida,
        DateTime fechaDevolucion,
        long? reservaIdExcluir = null,
        CancellationToken ct = default);
}
