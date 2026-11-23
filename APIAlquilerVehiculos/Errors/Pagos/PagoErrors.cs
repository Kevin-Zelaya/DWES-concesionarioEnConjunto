using APIAlquilerVehiculos.Errors.Common;

namespace APIAlquilerVehiculos.Errors.Pagos;

public static class PagoErrors
{
    public static NotFoundError ReservaNotFound(long id) =>
        NotFoundError.FromResource("Reserva", id.ToString());

    public static ConflictError ReservaNoPendiente() =>
        ConflictError.Business(
            "La reserva no se encuentra pendiente de pago.");
}