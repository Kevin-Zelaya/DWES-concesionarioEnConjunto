using System.Net;

namespace APIAlquilerVehiculos.Errors.Common;

public record  DomainError(string Message, HttpStatusCode StatusCode);

/// <summary>
/// Error 404 Not Found: El recurso solicitado no existe.
/// </summary>
public record NotFoundError(string Message, HttpStatusCode StatusCode) : DomainError(Message, StatusCode)
{
    public static NotFoundError FromResource(string resource, string id) =>
        new($"No existe {resource} con id {id}", HttpStatusCode.NotFound);
    
    public static NotFoundError Messagee(string message) =>
        new(message, HttpStatusCode.NotFound);
}

/// <summary>
/// Error 409 Conflict: Conflictos de estado.
/// </summary>
public sealed record ConflictError(string Message, HttpStatusCode StatusCode) : DomainError(Message, StatusCode)
{
    public static ConflictError Business(string message) => new(message, HttpStatusCode.Conflict);
    
    public static ConflictError Duplicate(string resourceType, string value) =>
        new($"Ya existe un {resourceType} con el valor '{value}'", HttpStatusCode.Conflict);
}

/// <summary>
/// Error 403 Forbidden: Usuario autenticado no tiene permisos sobre la ación.
/// </summary>
public sealed record ForbiddenError(string Message) : DomainError(Message, HttpStatusCode.Forbidden)
{
    public static ForbiddenError InsufficientPermissions(string action) =>
        new($"No tiene permisos suficientes para {action}");

    public static ForbiddenError ResourceAccessDenied(string resource) =>
        new($"No tiene acceso a este {resource}");
}

/// <summary>
/// Error 400 - 422: Incumplimiento de regla de negocio.
/// </summary>
public sealed record BusinessRuleError(string Message) : DomainError(Message, HttpStatusCode.BadRequest);

/// <summary>
/// Error 400 Bad Request: Fallos de validación de entrada, es decir fluentValidation o Annotations.
/// </summary>
public sealed record ValidationError(string Message, IDictionary<string, string[]> Errors): DomainError(Message, HttpStatusCode.BadRequest)
{
    public static ValidationError FromFluentValidation(IDictionary<string, string[]> errors) =>
        new("Errores de validación", errors);
    
    public static ValidationError Create(string message, IDictionary<string, string[]>? errors = null) =>
        new(message, errors ?? new Dictionary<string, string[]>());
}

/// <summary>
/// Error 401 Unauthorized: Fallos de autenticación o sesiones no válidas.
/// </summary>
public sealed record UnauthorizedError(string Message, HttpStatusCode StatusCode) : DomainError(Message, StatusCode)
{
    public static UnauthorizedError InvalidCredentials() => new("Credenciales inválidas", HttpStatusCode.Unauthorized);
    
    public static UnauthorizedError InactiveAccount() => new("La cuenta de usuario se encuentra desactivada", HttpStatusCode.Unauthorized);
}

/// <summary>
/// Error 500 Internal Server Error: Fallos técnicos del servidor.
/// </summary>
public sealed record InternalError(string Message) : DomainError(Message, HttpStatusCode.InternalServerError)
{
    public static InternalError ErrorServer(string message) => new(message);
}