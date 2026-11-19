using APIAlquilerVehiculos.Errors.Common;

namespace APIAlquilerVehiculos.Errors.Reservas;

public static class LineaErrors
{
    public static NotFoundError NotFound(long Id) => NotFoundError.FromResource("Linea de reserva", Id.ToString());
}