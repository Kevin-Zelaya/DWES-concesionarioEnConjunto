namespace APIAlquilerVehiculos.Dtos.Auth;

/// <summary>
/// Dto de salida. Respuesta al login de un usuario
/// </summary>
/// <param name="Token">Token del usuario logeado</param>
/// <param name="ExpiresAt">Fecha de expiración del token</param>
public record AuthResponseDto(
    string Token,
    DateTime ExpiresAt,
    UsuarioResponseDto Usuario
);