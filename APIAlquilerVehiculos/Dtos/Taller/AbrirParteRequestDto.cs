namespace APIAlquilerVehiculos.Dtos.Taller;

public record AbrirParteRequestDto(
    string VehiculoId,
    string Descripcion);