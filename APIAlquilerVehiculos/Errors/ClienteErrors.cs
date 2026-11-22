using APIAlquilerVehiculos.Errors.Common;

namespace APIAlquilerVehiculos.Errors;

public class ClienteErrors
{
    // 400 Bad Request
    public static ValidationError Validation(IDictionary<string, string[]> errors) =>
        ValidationError.FromFluentValidation(errors);

    public static ValidationError IdentityError(IEnumerable<string> identityErrors) =>
        ValidationError.Create("Error al crear el usuario", new Dictionary<string, string[]>
        {
            { "Identity", identityErrors.ToArray() }
        });

    // 409 Conflict
    public static ConflictError DuplicateDocument(string documento) =>
        ConflictError.Duplicate("Cliente", documento);

    public static ConflictError DuplicateEmail(string email) =>
        ConflictError.Duplicate("Usuario", email);

    // 401 Unauthorized
    public static UnauthorizedError InvalidCredentials() =>
        UnauthorizedError.InvalidCredentials();

    public static UnauthorizedError InactiveAccount() =>
        UnauthorizedError.InactiveAccount();

    // 404 Not Found
    public static NotFoundError UserNotFound(long id) =>
        NotFoundError.FromResource("Usuario", id.ToString());

    public static NotFoundError UserNotFound() =>
        NotFoundError.Messagee("Usuario autenticado no encontrado");
    
    //500 Internal Error 
    public static InternalError ErrorServer(string message) =>
        InternalError.ErrorServer(message);
}