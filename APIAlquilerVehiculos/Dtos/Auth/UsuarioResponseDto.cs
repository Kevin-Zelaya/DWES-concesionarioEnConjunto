namespace APIAlquilerVehiculos.Dtos.Auth;

public record UsuarioResponseDto (
    long Id,
    string Email, 
    string? Telefono,
    string Rol, 
    long? ClienteId,
    long? TrabajadorId, 
    DateTime CreatedAt
);