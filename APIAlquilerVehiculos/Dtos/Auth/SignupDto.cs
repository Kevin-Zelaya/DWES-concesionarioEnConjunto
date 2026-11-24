using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIAlquilerVehiculos.Dtos.Auth;


/// <summary>
/// Dto para el registro de un cliente
/// </summary>
/// <param name="Email">Email del cliente</param>
/// <param name="Telefono">Tlf del cliente</param>
/// <param name="Contraseña">Contraseña del cliente</param>
/// <param name="Nombre">Nombre del cliente</param>
/// <param name="Apellidos">Apellidos del cliente</param>
/// <param name="FechaNacimiento">Fecha de nacimiento del cliente</param>
/// <param name="Dni">Dni del cliente (opcional)</param>
/// <param name="Nie">Nie del cliente (opcional)</param>
/// <param name="Pasaporte">Pasaporte del cliente (opcional)</param>
/// <param name="NumeroPermisoConducir">Nº del permido de conducir del cliente si solo se presenta el pasaporte</param>
public record SignupDto (
    
    [property: JsonPropertyName("email")]
    [Required(ErrorMessage = "El email es obligatorio")] 
    [EmailAddress(ErrorMessage = "El formato del email no es válido")] 
    [MaxLength(255, ErrorMessage = "El email no puede superar los 255 caracteres")] 
    string Email,
    
    [property: JsonPropertyName("telefono")]
    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [Phone(ErrorMessage = "El formato del teléfono no es válido")] 
    [MaxLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres")] 
    string Telefono,
    
    [property: JsonPropertyName("contraseña")]
    [Required(ErrorMessage = "La contraseña es obligatoria")] 
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")] 
    [MaxLength(100, ErrorMessage = "La contraseña no puede superar los 100 caracteres")] 
    string Contraseña,
    
    [property: JsonPropertyName("nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio")] 
    [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")] 
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    string Nombre,
    
    [property: JsonPropertyName("apellidos")]
    [Required(ErrorMessage = "Los apellidos son obligatorios")] 
    [MinLength(2, ErrorMessage = "Los apellidos deben tener al menos 2 caracteres")] 
    [MaxLength(150, ErrorMessage = "Los apellidos no pueden superar los 150 caracteres")] 
    string Apellidos,
    
    [property: JsonPropertyName("fecha_nacimiento")]
    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")] 
    DateTime FechaNacimiento,
    
    // documentos opcionales
    
    [property: JsonPropertyName("dni")]
    [MaxLength(9, ErrorMessage = "El DNI debe tener un máximo de 9 caracteres")] 
    string? Dni,
    
    [property: JsonPropertyName("nie")]
    [MaxLength(9, ErrorMessage = "El NIE debe tener un máximo de 9 caracteres")] 
    string? Nie,
    
    [property: JsonPropertyName("pasaporte")]
    [MaxLength(20, ErrorMessage = "El pasaporte no puede superar los 20 caracteres")] 
    string? Pasaporte,
    
    // solo si presenta pasaporte:
    [property: JsonPropertyName("num_permiso_conducir")]
    [MaxLength(30, ErrorMessage = "El número de permiso no puede superar los 30 caracteres")]
    string? NumeroPermisoConducir
);