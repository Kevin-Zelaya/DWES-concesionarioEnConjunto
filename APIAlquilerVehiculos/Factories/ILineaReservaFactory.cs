using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

/// <summary>
/// Define el contrato para la fábrica encargada de crear líneas de reserva.
/// </summary>
public interface ILineaReservaFactory
{
    /// <summary>
    /// Crea una línea de reserva según el tipo indicado en la solicitud.
    /// </summary>
    /// <param name="request">Datos necesarios para crear la línea de reserva.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Resultado con la línea de reserva creada o el error producido.</returns>
    Task<Result<LineaReserva, DomainError>> CreateAsync(LineaReservaCreateRequestDto request, CancellationToken ct = default);
}