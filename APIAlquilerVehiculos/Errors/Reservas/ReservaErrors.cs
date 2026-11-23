using System.Net;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Models.Reservas;

namespace APIAlquilerVehiculos.Errors.Reservas;

public static class ReservaErrors
{
    public static NotFoundError NotFound(string CodigoReferencia) => NotFoundError.FromResource("Reserva", CodigoReferencia);
    public static NotFoundError NotFound(long Id) => NotFoundError.FromResource("Reserva", Id.ToString());
    public static DomainError HorarioNoValido()
    {
        return new DomainError("La fecha y hora de recogida o devolución deben estar dentro del horario de atención de la oficina.", HttpStatusCode.BadRequest);
    }
    public static ConflictError TransicionEstadoNoValida(Estado estadoActual, Estado nuevoEstado)
    {
        return new ConflictError($"No se permite cambiar el estado de la reserva de {estadoActual} a {nuevoEstado}.", HttpStatusCode.Conflict);
    }
}