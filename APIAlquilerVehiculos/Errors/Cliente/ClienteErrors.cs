using APIAlquilerVehiculos.Errors.Common;

namespace APIAlquilerVehiculos.Errors;

public class ClienteErrors
{
    // 400 Bad Request
    public static ValidationError Validation(IDictionary<string, string[]> errors) =>
        ValidationError.FromFluentValidation(errors);

    public static ValidationError IdentityError(IEnumerable<string> identityErrors) =>
        ValidationError.Create("Error al gestionar el usuario del cliente", new Dictionary<string, string[]>
        {
            { "Identity", identityErrors.ToArray() }
        });

    // 404 Not Found
    public static NotFoundError NotFound(long id) =>
        NotFoundError.FromResource("Cliente", id.ToString());
    
    public static NotFoundError UserNotFound() =>
        NotFoundError.Messagee("Usuario autenticado no encontrado");

    public static ForbiddenError PermisosInsuficientes(string message) =>
        ForbiddenError.InsufficientPermissions(message);

    // 409 Conflict (duplicados)
    public static ConflictError DuplicateDocument(string documento) =>
        ConflictError.Duplicate("Cliente", documento);

    public static ConflictError DuplicateEmail(string email) =>
        ConflictError.Duplicate("Usuario", email);
    
    public static ConflictError DuplicateDniOrEmail() =>
        ConflictError.Business("Ya existe un cliente o usuario con ese DNI o email");
    
    public static ConflictError YaInactivo(long id) =>
        ConflictError.Business($"El cliente con id {id} ya está dado de baja");
    
    public static ConflictError YaActivo(long id) =>
        ConflictError.Business($"El cliente con id {id} está activo");
    
    public static ConflictError TieneReservasVivas(long id) =>
        ConflictError.Business($"No se puede dar de baja al cliente {id}: tiene reservas vivas (Pendiente, Confirmada o EnCurso). Resuélvalas o reasígnelas primero");

    // 500 Internal Error
    public static InternalError ErrorServer(string message) =>
        InternalError.ErrorServer(message);
}