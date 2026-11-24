using APIAlquilerVehiculos.DTOs.Pagos;
using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

namespace APIAlquilerVehiculos.Services.Pagos;

public interface IPagoService
{
    Task<Result<PagoResponse, DomainError>> CrearPagoAsync(
        long reservaId,
        CrearPagoRequestDto request,
        CancellationToken ct = default);
}