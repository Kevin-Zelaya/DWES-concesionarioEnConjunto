using APIAlquilerVehiculos.Models.Enums;

public static class EstadoVehiculoExtensions
{
    public static bool PuedeTransicionarA(
        this EstadoEnum estadoActual,
        EstadoEnum nuevoEstado)
    {
        return estadoActual switch
        {
            EstadoEnum.Disponible =>
                nuevoEstado == EstadoEnum.Reservado ||
                nuevoEstado == EstadoEnum.Retirado,

            EstadoEnum.Reservado =>
                nuevoEstado == EstadoEnum.Disponible ||
                nuevoEstado == EstadoEnum.EnAlquiler,

            EstadoEnum.EnAlquiler =>
                nuevoEstado == EstadoEnum.Disponible ||
                nuevoEstado == EstadoEnum.Reservado ||
                nuevoEstado == EstadoEnum.EnMantenimiento,

            EstadoEnum.Retirado =>
                nuevoEstado == EstadoEnum.EnMantenimiento,

            EstadoEnum.EnMantenimiento =>
                nuevoEstado == EstadoEnum.Disponible,

            _ => false
        };
    }
}