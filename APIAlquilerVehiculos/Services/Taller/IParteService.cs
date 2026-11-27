using APIAlquilerVehiculos.Dtos.Taller;
using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

namespace APIAlquilerVehiculos.Services.Taller;

public interface IParteService
{
    Task<Result<List<ParteResponseDto>, DomainError>> GetAllAsync();
    Task<Result<ParteResponseDto, DomainError>> GetByIdAsync(long id);
    Task<Result<ParteResponseDto, DomainError>> AbrirParteAsync(long trabajadorId, AbrirParteRequestDto request);
    Task<Result<ParteResponseDto, DomainError>> CerrarParteAsync(long trabajadorId, long parteId);

    Task NotificarReservaAfectadaTallerAsync(
        string emailCliente,
        string nombreCliente,
        string codigoReferencia,
        Models.Vehiculo vehiculoOriginal,
        string descripcionAveria,
        string resolucionAplicada,
        string detalleResolucion);
}