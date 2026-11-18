using APIAlquilerVehiculos.Errors.Common;

namespace APIAlquilerVehiculos.Errors.Reservas;

public static class ReservaErrors
{
    public static NotFoundError NotFound(string CodigoReferencia) => NotFoundError.FromResource("Reserva", CodigoReferencia);
    public static NotFoundError NotFound(long Id) => NotFoundError.FromResource("Reserva", Id.ToString());
}