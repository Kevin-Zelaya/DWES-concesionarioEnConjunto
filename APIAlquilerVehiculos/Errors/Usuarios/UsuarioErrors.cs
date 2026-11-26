using APIAlquilerVehiculos.Errors.Common;

namespace APIAlquilerVehiculos.Errors.Usuarios;

public class UsuarioErrors
{
    // 404 Not Found
    public static NotFoundError NotFound(long id) =>
        NotFoundError.FromResource("Usuario", id.ToString());

}