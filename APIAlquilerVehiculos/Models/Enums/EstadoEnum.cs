namespace APIAlquilerVehiculos.Models.Enums;

/// <summary>
/// Posibles estados de un vehículo en el sistema de alquiler.
/// </summary>
public enum EstadoEnum
{
    Disponible,
    Reservado,
    EnMantenimiento,
    EnAlquiler,
    Retirado
}