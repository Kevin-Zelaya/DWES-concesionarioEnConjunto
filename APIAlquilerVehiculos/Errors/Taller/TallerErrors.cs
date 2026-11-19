using System.Net;
using APIAlquilerVehiculos.Errors.Common;

namespace APIAlquilerVehiculos.Errors.Taller;

public static class TallerErrors
{
    public static DomainError SoloMecanicos() =>
        new UnauthorizedError("Solo un trabajador activo con perfil Mecánico puede tramitar partes de taller.", HttpStatusCode.Forbidden);

    public static DomainError VehiculoRetirado(string matricula) =>
        new ConflictError($"El vehículo {matricula} está retirado y no puede entrar en taller.", HttpStatusCode.Conflict);

    public static DomainError ParteYaAbierto(string matricula) =>
        new ConflictError($"El vehículo {matricula} ya tiene un parte de taller abierto.", HttpStatusCode.Conflict);

    public static DomainError ParteYaCerrado(long parteId) =>
        new ConflictError($"El parte {parteId} ya se encuentra cerrado.", HttpStatusCode.Conflict);

    public static DomainError VehiculoConReservasActivas(string matricula, IEnumerable<string> codigosReserva) =>
        new ConflictError(
            $"No se puede enviar el vehículo {matricula} a taller porque tiene reservas activas sin resolver: {string.Join(", ", codigosReserva)}.",
            HttpStatusCode.Conflict);
}