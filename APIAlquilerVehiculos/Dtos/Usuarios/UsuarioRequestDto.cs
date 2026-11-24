using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIAlquilerVehiculos.Dtos.Usuarios;

public record UsuarioContraseñaDto
(
   [property: JsonPropertyName("contrasenaNueva")]
    [Required(ErrorMessage = "La contraseña nueva es obligatoria")] 
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")] 
    [MaxLength(100, ErrorMessage = "La contraseña no puede superar los 100 caracteres")] 
    string ContraseñaNueva,
    
    [property: JsonPropertyName("confirmarContrasena")]
    [Required(ErrorMessage = "La contraseña nueva es obligatoria")] 
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")] 
    [MaxLength(100, ErrorMessage = "La contraseña no puede superar los 100 caracteres")] 
    string Confirmacion
);

public record UsuarioRolDto
{
    public required string Rol { get; set; }
}