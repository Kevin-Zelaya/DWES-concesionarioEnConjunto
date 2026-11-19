namespace APIAlquilerVehiculos.Dtos.Trabajadores;

/// <summary>
/// Dto de salida de un trabajador
/// </summary>
// sin validaciones porque es de salida
public record ResponseTrabajadorDto(
    long Id,
    long IdUsuario,
    string Nombre,
    string Apellidos,
    string Email,
    string? Telefono,
    string Dni,
    DateTime FechaIngreso,
    string TipoTrabajador,
    bool IsActivo,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);