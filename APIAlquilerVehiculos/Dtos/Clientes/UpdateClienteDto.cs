using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace APIAlquilerVehiculos.Dtos.Cientes;

[SwaggerSchema(Description = "Datos para actualizar el cliente.")]
public record UpdateClienteDto( 

    [SwaggerSchema(Description = "Identificador del cliente a actualizar", Nullable = false)]
    long Id,
    
    [property: JsonPropertyName("nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio")] 
    [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")] 
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    [SwaggerSchema(Description = "Nombre del cliente a actualizar", Nullable = false)]
    string Nombre,
    
    [property: JsonPropertyName("apellidos")]
    [Required(ErrorMessage = "Los apellidos son obligatorios")] 
    [MinLength(2, ErrorMessage = "Los apellidos deben tener al menos 2 caracteres")] 
    [MaxLength(150, ErrorMessage = "Los apellidos no pueden superar los 150 caracteres")] 
    [SwaggerSchema(Description = "Apellidos del cliente a actualizar", Nullable = false)]
    string Apellidos,
    
    [property: JsonPropertyName("email")]
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [MaxLength(255, ErrorMessage = "El email no puede superar los 255 caracteres")]
    [SwaggerSchema(Description = "Correo electronico del cliente a actualizar", Nullable = false)]
    string Email,

    [property: JsonPropertyName("telefono")]
    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    [MaxLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres")]
    [SwaggerSchema(Description = "Telefono del cliente a actualizar", Nullable = false)]
    string Telefono,
    
    [property: JsonPropertyName("fechaNacimiento")]
    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")] 
    [SwaggerSchema(Description = "Fecha de nacimiento del cliente a actualizar", Nullable = false)]
    DateTime FechaNacimiento,
    
    // documentos opcionales
    [property: JsonPropertyName("dni")]
    [MaxLength(9, ErrorMessage = "El DNI debe tener un máximo de 9 caracteres")] 
    [SwaggerSchema(Description = "Documento Nacional de Identidad del cliente a actualizar", Nullable = true)]
    string? Dni,
    
    [property: JsonPropertyName("nie")]
    [MaxLength(9, ErrorMessage = "El NIE debe tener un máximo de 9 caracteres")] 
    [SwaggerSchema(Description = "Número de Identidad de Extranjero del cliente a actualizar", Nullable = true)]
    string? Nie,
    
    [property: JsonPropertyName("pasaporte")]
    [MaxLength(20, ErrorMessage = "El pasaporte no puede superar los 20 caracteres")] 
    [SwaggerSchema(Description = "Número de pasaporte del cliente a actualizar", Nullable = true)]
    string? Pasaporte,
    
    // solo si presenta pasaporte:
    [property: JsonPropertyName("numeroPermisoConducir")]
    [MaxLength(30, ErrorMessage = "El número de permiso no puede superar los 30 caracteres")]
    [SwaggerSchema(Description = "Número de permiso de conducir del cliente a actualizar", Nullable = true)]
    string? NumeroPermisoConducir
);