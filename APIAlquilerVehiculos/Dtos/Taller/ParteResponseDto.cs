using APIAlquilerVehiculos.Models;

namespace APIAlquilerVehiculos.Dtos.Taller;

public record ParteResponseDto(
    long Id,
    DateTime FechaApertura,
    DateTime? FechaCierre,
    long QuienLoTramitaId,
    string Descripcion,
    string VehiculoId,
    string MatriculaVehiculo,
    EstadoParte Estado);