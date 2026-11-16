using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIAlquilerVehiculos.Dtos.Auth;

/// <summary>
/// Dto de entrada para el login de los users
/// </summary>
/// <param name="Email">Email para el inicio de sesión</param>
/// <param name="Contraseña">Contraseña asociada al correo</param>
public record LoginDto(
    
    [property: JsonPropertyName("email")]
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [MaxLength(255, ErrorMessage = "El email no puede superar los 255 caracteres")]
    string Email,
    
    [property: JsonPropertyName("contraseña")]
    [Required(ErrorMessage = "La contraseña es obligatoria")] 
    string Contraseña
);