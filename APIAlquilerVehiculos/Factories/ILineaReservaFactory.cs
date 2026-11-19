using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

public interface ILineaReservaFactory
{
    Task<Result<LineaReserva, DomainError>> CreateAsync(LineaReservaCreateRequestDto request, CancellationToken ct = default);
}