using APIAlquilerVehiculos.Errors.Common;

namespace APIAlquilerVehiculos.Errors.Trabajador;

public static class TrabajadorErrors
{
    // 400 Bad Request
    public static ValidationError Validation(IDictionary<string, string[]> errors) =>
        ValidationError.FromFluentValidation(errors);

    public static ValidationError IdentityError(IEnumerable<string> identityErrors) =>
        ValidationError.Create("Error al gestionar el usuario del trabajador", new Dictionary<string, string[]>
        {
            { "Identity", identityErrors.ToArray() }
        });

    // 404 Not Found
    public static NotFoundError NotFound(long id) =>
        NotFoundError.FromResource("Trabajador", id.ToString());

    // 409 Conflict (duplicados)
    public static ConflictError DuplicateDni(string dni) =>
        ConflictError.Duplicate("Trabajador", dni);

    public static ConflictError DuplicateEmail(string email) =>
        ConflictError.Duplicate("Usuario", email);

    public static ConflictError DuplicateDniOrEmail() =>
        ConflictError.Business("Ya existe un trabajador o usuario con ese DNI o email");

    // 409 Conflict 
    public static ConflictError YaInactivo(long id) =>
        ConflictError.Business($"El trabajador con id {id} ya está dado de baja");

    public static ConflictError EsResponsable(long id) =>
        ConflictError.Business(
            $"No se puede dar de baja al trabajador {id}: es el responsable de la oficina. Nombre antes a otro responsable");

    public static ConflictError TieneReservasVivas(long id) =>
        ConflictError.Business(
            $"No se puede dar de baja al trabajador {id}: tiene reservas vivas (Pendiente, Confirmada o EnCurso). Resuélvalas o reasígnelas primero");

    // 500 Internal Error
    public static InternalError ErrorServer(string message) =>
        InternalError.ErrorServer(message);
}