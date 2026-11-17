using System.Net;
using APIAlquilerVehiculos.Errors.Common;

namespace APIAlquilerVehiculos.Errors.Email;

public sealed record EmailErrors(string Message, HttpStatusCode StatusCode) : DomainError(Message, StatusCode)
{
    public static EmailErrors InvalidAddress(string email) =>
        new($"La dirección de correo '{email}' no es válida.", HttpStatusCode.BadRequest);

    public static EmailErrors SendFailed(string detail) =>
        new($"Error al enviar el correo electrónico: {detail}", HttpStatusCode.BadGateway);
}