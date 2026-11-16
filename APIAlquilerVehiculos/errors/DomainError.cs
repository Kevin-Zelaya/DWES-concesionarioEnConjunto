using System.Net;

/// <summary>
/// Representa el error base de dominio en la aplicación, incluyendo un mensaje descriptivo y su código de estado HTTP asociado.
/// </summary>
/// <param name="Message">Mensaje descriptivo del error.</param>
/// <param name="StatusCode">Código de estado HTTP que corresponde al error.</param>
public record DomainError(string Message, HttpStatusCode StatusCode);

/// <summary>
/// Representa un error de tipo "No Encontrado" (404 Not Found) cuando un recurso solicitado no existe en el sistema.
/// </summary>
public record NotFoundError(string Message, HttpStatusCode StatusCode) : DomainError(Message, StatusCode)
{
    /// <summary>
    /// Crea una instancia predeterminada de <see cref="NotFoundError"/> cuando no se encuentra un recurso específico por su identificador.
    /// </summary>
    /// <param name="resource">Nombre del recurso que no ha sido encontrado.</param>
    /// <param name="id">Identificador del recurso faltante.</param>
    /// <returns>Una nueva instancia de <see cref="NotFoundError"/> con el mensaje formateado y código HTTP 404.</returns>
    public static NotFoundError FromResource(string resource, string id) =>
        new($"No existe {resource} con id {id}", HttpStatusCode.NotFound);
}

/// <summary>
/// Representa un error de conflicto (409 Conflict) cuando se intenta registrar o modificar un recurso duplicado.
/// </summary>
public sealed record ConflictError(string Message, HttpStatusCode StatusCode) : DomainError(Message, StatusCode)
{
    /// <summary>
    /// Crea una instancia predeterminada de <see cref="ConflictError"/> para un recurso que ya está registrado con un valor único.
    /// </summary>
    /// <param name="resourceType">Tipo de recurso que genera el conflicto.</param>
    /// <param name="value">Valor duplicado que provocó el conflicto.</param>
    /// <returns>Una nueva instancia de <see cref="ConflictError"/> con código HTTP 409.</returns>
    public static ConflictError Duplicate(string resourceType, string value) =>
        new($"Ya existe un {resourceType} con el valor '{value}'", HttpStatusCode.Conflict);
}

/// <summary>
/// Representa un error por incumplimiento de reglas de negocio (400 Bad Request).
/// </summary>
public sealed record BusinessRuleError(string Message) : DomainError(Message, HttpStatusCode.BadRequest);

/// <summary>
/// Representa un error de autorización o autenticación fallida (401 Unauthorized).
/// </summary>
public sealed record UnauthorizedError(string Message, HttpStatusCode StatusCode) : DomainError(Message, StatusCode)
{
    /// <summary>
    /// Crea una instancia estándar para credenciales de acceso inválidas.
    /// </summary>
    public static UnauthorizedError InvalidCredentials() => new("Credenciales inválidas", HttpStatusCode.Unauthorized);
}

/// <summary>
/// Representa un error interno del servidor no controlado (500 Internal Server Error).
/// </summary>
public sealed record InternalError(string Message) : DomainError(Message, HttpStatusCode.InternalServerError);